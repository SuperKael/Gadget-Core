using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GadgetCore.Util
{
    /// <summary>
    /// Reflection utility used by the /reflect command.
    /// </summary>
    public class Reflector
    {
        private Dictionary<string, Type> resolvedTypes = new Dictionary<string, Type>();
        private Dictionary<string, MemberInfo> resolvedMembers = new Dictionary<string, MemberInfo>();
        private Dictionary<string, Tuple<MemberInfo, object>> resolvedRefs = new Dictionary<string, Tuple<MemberInfo, object>>();

        /// <summary>
        /// Reads the specified member or reference's value as a string.
        /// </summary>
        public string ReadValue(string identifier, string instance)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Member Read Failed: Invalid Identifier");
            Tuple<MemberInfo, object> iRef;
            try
            {
                iRef = ResolveReference(identifier, instance);
            }
            catch (ReflectorException e)
            {
                throw new ReflectorException("Member Read Failed: " + e.Message, e);
            }
            try
            {
                object val = iRef.Item1.GetValue(iRef.Item2);
                if (val == null) return "null";
                else if (val == iRef.Item1) throw new ReflectorException(iRef.Item1.MemberType + " Read Failed: Reading Not Possible For " + iRef.Item1.MemberType);
                return val is string str ? str : val is IEnumerable enumerable ? enumerable.Cast<object>().RecursiveConcat() : val.ToString();
            }
            catch (ReflectorException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new ReflectorException(iRef.Item1.MemberType + " Read Failed: " + e.Message, e);
            }
        }

        /// <summary>
        /// Attempts to write the specified member or reference's value as a string. <paramref name="value"/> can be a literal value, or a reference.
        /// </summary>
        public string WriteValue(string identifier, string instance, string value)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Member Write Failed: Invalid Identifier");
            Tuple<MemberInfo, object> iRef;
            try
            {
                iRef = ResolveReference(identifier, instance);
            }
            catch (ReflectorException e)
            {
                throw new ReflectorException("Member Write Failed: " + e.Message, e);
            }
            Type setType = iRef.Item1.GetSetType();
            if (setType == null) throw new ReflectorException(iRef.Item1.MemberType + " Write Failed: Writing Not Possible For " + iRef.Item1.MemberType);
            try
            {
                object setValue;
                if (string.IsNullOrEmpty(value) || value == "null")
                {
                    setValue = null;
                }
                else if (value[0] == '$')
                {
                    Tuple<MemberInfo, object> pRef;
                    try
                    {
                        pRef = ResolveReference(value, null);
                    }
                    catch (ReflectorException e)
                    {
                        throw new ReflectorException("Method Invoke Failed: Parameter Resolution Failed: " + e.Message, e);
                    }
                    setValue = pRef.Item1.GetValue(pRef.Item2);
                }
                else
                {
                    setValue = value;
                }
                if (setValue != null && !setType.IsAssignableFrom(setValue.GetType()))
                {
                    setValue = Convert.ChangeType(setValue, setType);
                }
                iRef.Item1.SetValue(iRef.Item2, setValue);
                object val = iRef.Item1.GetValue(iRef.Item2);
                if (val == null) return "null";
                return val is string str ? str : val is IEnumerable enumerable ? enumerable.Cast<object>().RecursiveConcat() : val.ToString();
            }
            catch (InvalidCastException e)
            {
                throw new ReflectorException(iRef.Item1.MemberType + " Write Failed: Cannot Write To " + iRef.Item1.MemberType + " Type", e);
            }
            catch (Exception e)
            {
                throw new ReflectorException(iRef.Item1.MemberType + " Write Failed: " + e.Message, e);
            }
        }

        /// <summary>
        /// Attempts to invoke a method or property with an array of args as strings.
        /// Returns "void" if invoked method has a return type of void.
        /// </summary>
        public string Invoke(string identifier, string instance, params string[] args)
        {
            return InvokeReturn(identifier, instance, null, args);
        }

        /// <summary>
        /// Attempts to invoke a method or property with an array of args as strings, and stores the return value at a given target.
        /// Returns "void" if invoked method has a return type of void.
        /// </summary>
        public string InvokeReturn(string identifier, string instance, string returnTarget, params string[] args)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Method Invoke Failed: Invalid Identifier");
            if (args == null) throw new ReflectorException("Method Invoke Failed: Null Args");
            Tuple<MemberInfo, object> iRef;
            try
            {
                iRef = ResolveReference(identifier, instance);
            }
            catch (ReflectorException e)
            {
                throw new ReflectorException("Method Invoke Failed: " + e.Message, e);
            }
            try
            {
                MethodBase method = iRef.Item1 is MethodBase mb ? mb :
                                    iRef.Item1 is PropertyInfo pi ? (args.Length == 0 ? pi.GetGetMethod(true) : args.Length == 1 ? pi.GetSetMethod(true) :
                                    throw new ReflectorException("Method Invoke Failed: Argument Count Invalid For Property")) ??
                                    throw new ReflectorException("Method Invoke Failed: Property Method Does Not Exist") :
                                    throw new ReflectorException("Method Invoke Failed: Identifier Is Not A Invokable");
                ParameterInfo[] methodParams = method.GetParameters();
                if (args.Length != methodParams.Length) throw new ReflectorException("Method Invoke Failed: Incorrect Argument Count - Expected " + methodParams.Length + ", Got " + args.Length);
                object[] methodArgs = new object[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.IsNullOrEmpty(args[i]) || args[i] == "null")
                    {
                        methodArgs[i] = null;
                    }
                    else if (args[i][0] == '$')
                    {
                        Tuple<MemberInfo, object> pRef;
                        try
                        {
                            pRef = ResolveReference(args[i], null);
                        }
                        catch (ReflectorException e)
                        {
                            throw new ReflectorException("Method Invoke Failed: Parameter Resolution Failed: " + e.Message, e);
                        }
                        methodArgs[i] = pRef.Item1.GetValue(pRef.Item2);
                    }
                    else
                    {
                        methodArgs[i] = args[i];
                    }
                    if (methodArgs[i] != null && !methodParams[i].ParameterType.IsAssignableFrom(methodArgs[i].GetType()))
                    {
                        methodArgs[i] = Convert.ChangeType(methodArgs[i], methodParams[i].ParameterType);
                    }
                }
                object val;
                if (returnTarget != null)
                {
                    if (string.IsNullOrEmpty(returnTarget)) throw new ReflectorException("Method Invoke Failed: Return Reference Resolution Failed: Invalid Identifier");
                    MethodInfo methodInfo = method as MethodInfo;
                    if (methodInfo != null && methodInfo.ReturnType == typeof(void)) throw new ReflectorException("Method Invoke Failed: Method Returns Void");
                    if (!resolvedRefs.TryGetValue(returnTarget, out Tuple<MemberInfo, object> oRef))
                    {
                        if (returnTarget[0] == '$' && !string.IsNullOrEmpty(returnTarget = returnTarget?.TrimStart('$')))
                        {
                            Type genericTemporaryReferenceContainerType = typeof(TemporaryReferenceContainer<>).MakeGenericType(methodInfo != null ? methodInfo.ReturnType : methodInfo.DeclaringType);
                            resolvedRefs.Add("$" + returnTarget, oRef = Tuple.Create<MemberInfo, object>(genericTemporaryReferenceContainerType.GetField("value"), Activator.CreateInstance(genericTemporaryReferenceContainerType)));
                        }
                        else throw new ReflectorException("Method Invoke Failed: Return Reference Resolution Failed: Invalid Identifier");
                    }
                    val = method.Invoke(iRef.Item2, methodArgs);
                    try
                    {
                        oRef.Item1.SetValue(oRef.Item2, val);
                    }
                    catch (Exception e)
                    {
                        throw new ReflectorException("Method Invoke Failed: Return " + oRef.Item1.MemberType + " Write Failed: " + e.Message, e);
                    }
                }
                else
                {
                    val = method.Invoke(iRef.Item2, methodArgs);
                }
                if (method is MethodInfo mi && mi.ReturnType == typeof(void)) return "void";
                if (val == null) return "null"; 
                return val is string str ? str : val is IEnumerable enumerable ? enumerable.Cast<object>().RecursiveConcat() : val.ToString();
            }
            catch (InvalidCastException e)
            {
                throw new ReflectorException("Method Invoke Error: Write Failed: Cannot Write To " + iRef.Item1.MemberType + " Type", e);
            }
            catch (TargetInvocationException e)
            {
                throw new ReflectorException("Method Invoke Error: Exception Thrown By Method: " + e.InnerException, e);
            }
            catch (Exception e)
            {
                throw new ReflectorException("Method Invoke Failed: " + e.Message, e);
            }
        }

        /// <summary>
        /// 'Uses' a namespace, thereby simplifying the use of <see cref="ResolveType(string)"/>.
        /// </summary>
        public void UseNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) throw new ReflectorException("Namespace Usage Failed: Invalid Namespace");
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(x => x?.Namespace?.Equals(ns, StringComparison.CurrentCultureIgnoreCase) == true));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types.AddRange(ex.Types.Where(x => x != null && x?.Namespace?.Equals(ns, StringComparison.CurrentCultureIgnoreCase) == true));
                }
            }
            foreach (Type t in types)
            {
                resolvedTypes[t.Name] = t;
            }
        }

        /// <summary>
        /// 'Unuses' a namespace, potentially resolving name conflicts with the use of <see cref="ResolveType(string)"/>.
        /// Also removes cached type resolutions from this namespace, even if this namespace was not previously 'Used'.
        /// </summary>
        public void UnuseNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) throw new ReflectorException("Namespace Usage Failed: Invalid Namespace");
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(x => x?.Namespace?.Equals(ns, StringComparison.CurrentCultureIgnoreCase) == true));
                }
                catch (ReflectionTypeLoadException e)
                {
                    types.AddRange(e.Types.Where(x => x != null && x?.Namespace?.Equals(ns, StringComparison.CurrentCultureIgnoreCase) == true));
                }
            }
            foreach (Type t in types)
            {
                resolvedTypes.Remove(t.Name);
            }
        }

        /// <summary>
        /// Resolves a type using an identifier.
        /// </summary>
        public Type ResolveType(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Type Resolution Failed: Invalid Identifier");
            try
            {
                if (resolvedTypes.TryGetValue(identifier, out Type t))
                {
                    return t;
                }
                string[] splitIdentifier = identifier.Split(':');
                switch (splitIdentifier.Length)
                {
                    case 1:
                        t = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(identifier, false, true)).SingleOrDefault(x => x != null);
                        break;
                    case 2:
                        identifier = splitIdentifier[1];
                        if (resolvedTypes.TryGetValue(identifier, out t) && t.Assembly.GetName().Name.Equals(splitIdentifier[0]))
                        {
                            return t;
                        }
                        t = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name.Equals(splitIdentifier[0])).GetType(identifier, false, true);
                        break;
                    default:
                        throw new ReflectorException("Type Resolution Failed: Invalid Identifier");
                }
                if (t == null) throw new ReflectorException("Type Resolution Failed: Type Does Not Exist: " + identifier);
                resolvedTypes[t.Name] = t;
                resolvedTypes[identifier] = t;
                return t;
            }
            catch (ReflectorException)
            {
                throw;
            }
            catch (InvalidOperationException e)
            {
                if (e.Message.Equals("Sequence contains more than one element")) throw new ReflectorException("Type Resolution Failed: Ambiguous Identifier");
                else if (e.Message.Equals("Sequence contains no matching element")) throw new ReflectorException("Type Resolution Failed: Assembly Does Not Exist");
                else throw new ReflectorException("Type Resolution Failed: InvalidOperationException: " + e.Message, e);
            }
            catch (Exception e)
            {
                throw new ReflectorException("Type Resolution Failed: " + e.GetType().Name + ": " + e.Message, e);
            }
        }

        /// <summary>
        /// Resolves a member using an identifier
        /// </summary>
        public MemberInfo ResolveMember(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Member Resolution Failed: Invalid Identifier");
            if (resolvedMembers.TryGetValue(identifier, out MemberInfo m))
            {
                return m;
            }
            int dotIndex = identifier.LastIndexOf('.');
            if (dotIndex < 0) throw new ReflectorException("Member Resolution Failed: Identifier Missing Type");
            Type t;
            try
            {
                t = ResolveType(identifier.Substring(0, dotIndex));
            }
            catch (ReflectorException e)
            {
                throw new ReflectorException("Member Resolution Failed: " + e.Message, e);
            }
            try
            {
                string memberName = identifier.Substring(dotIndex + 1);
                string fullIdentifier = t.FullName + "." + memberName;
                MemberInfo[] mArr = t.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (mArr.Length == 0) throw new ReflectorException("Member Resolution Failed: Member Does Not Exist: " + fullIdentifier);
                m = mArr.Length > 1 && typeof(MethodBase).IsAssignableFrom(mArr.GetType().GetElementType()) ? mArr.FirstOrDefault(x => x is MethodBase mb && mb.GetParameters()?.Length == 1) ?? mArr[0] : mArr[0];
                if (m == null) throw new ReflectorException("Member Resolution Failed: Member Does Not Exist: " + fullIdentifier);
                resolvedMembers[memberName] = m;
                resolvedMembers[identifier] = m;
                resolvedMembers[fullIdentifier] = m;
                return m;
            }
            catch (ReflectorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ReflectorException("Member Resolution Failed: " + e.GetType().Name + ": " + e.Message, e);
            }
        }

        /// <summary>
        /// Resolves a reference using an identifier, an optional reference to the container object if referencing a member identifier,
        /// and an optional name to save the reference as in that case.
        /// </summary>
        public Tuple<MemberInfo, object> ResolveReference(string identifier, string instance, string name = null)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ReflectorException("Reference Resolution Failed: Invalid Identifier");
            if (resolvedRefs.TryGetValue(identifier, out Tuple<MemberInfo, object> oRef))
            {
                return oRef;
            }
            if (identifier[0] != '$')
            {
                MemberInfo m;
                try
                {
                    m = ResolveMember(identifier);
                }
                catch (ReflectorException e)
                {
                    throw new ReflectorException("Reference Resolution Failed: " + e.Message, e);
                }
                try
                {
                    if (!m.IsStatic())
                    {
                        if (string.IsNullOrEmpty(instance)) throw new ReflectorException("Reference Resolution Failed: Invalid Instance");
                        try
                        {
                            oRef = ResolveReference(instance.Equals("null", StringComparison.CurrentCultureIgnoreCase) ? null : instance, null);
                        }
                        catch (ReflectorException e)
                        {
                            throw new ReflectorException("Reference Resolution Failed: Instance Resolution Failed: " + e.Message, e);
                        }
                    }
                    oRef = Tuple.Create(m, oRef?.Item1.GetValue(oRef.Item2));
                    if (!string.IsNullOrEmpty(name = name?.TrimStart('$')))
                    {
                        resolvedRefs["$" + name] = oRef;
                    }
                    return oRef;
                }
                catch (ReflectorException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ReflectorException("Reference Resolution Failed: " + e.GetType().Name + ": " + e.Message, e);
                }
            }
            else
            {
                throw new ReflectorException("Reference Resolution Failed: Reference Does Not Exist: " + identifier);
            }
        }
        
        /// <summary>
        /// The only exception that should ever be thrown by the Reflector. Will contain a message clearly explaining the problem.
        /// </summary>
        public class ReflectorException : InvalidOperationException
        {
            /// <summary>
            /// Constructs a new ReflectorException
            /// </summary>
            public ReflectorException(string message) : base(message) { }

            /// <summary>
            /// Constructs a new ReflectorException
            /// </summary>
            public ReflectorException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class TemporaryReferenceContainer<T>
        {
#pragma warning disable 0649
            public T value;
#pragma warning restore 0649
        }
    }
}
