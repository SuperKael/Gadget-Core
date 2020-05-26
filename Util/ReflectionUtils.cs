using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GadgetCore.Util
{
    /// <summary>
    /// Provides the extension method 'InvokeMethod' for easily invoking methods with Reflection.
    /// </summary>
    public static class ReflectionUtils
    {
        private static Dictionary<MethodInfoData, MethodInfo> cachedMethods = new Dictionary<MethodInfoData, MethodInfo>();

        /// <summary>
        /// Invokes the method with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters to run the method with.</param>
        /// <returns>The value returned by the invoked method.</returns>
        public static T InvokeMethod<T>(this object type, string methodName, params object[] parameters)
        {
            return (T)InvokeMethod(type, methodName, null, parameters);
        }

        /// <summary>
        /// Invokes the method with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="generics">The generic types that the target method uses</param>
        /// <param name="parameters">The parameters to run the method with.</param>
        /// <returns>The value returned by the invoked method.</returns>
        public static T InvokeMethod<T>(this object type, string methodName, Type[] generics, params object[] parameters)
        {
            return (T)InvokeMethod(type, methodName, generics, parameters);
        }

        /// <summary>
        /// Invokes the method with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters to run the method with.</param>
        /// <returns>The value returned by the invoked method.</returns>
        public static object InvokeMethod(this object type, string methodName, params object[] parameters)
        {
            return InvokeMethod(type, methodName, null, parameters);
        }

        /// <summary>
        /// Invokes the method with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="generics">The generic types that the target method uses</param>
        /// <param name="parameters">The parameters to run the method with.</param>
        /// <returns>The value returned by the invoked method.</returns>
        public static object InvokeMethod(this object type, string methodName, Type[] generics, params object[] parameters)
        {
            MethodInfoData methodInfoData = new MethodInfoData(type.GetType(), methodName, generics, parameters.Select(x => x?.GetType()).ToArray());
            if (!cachedMethods.ContainsKey(methodInfoData))
            {
                cachedMethods.Add(methodInfoData, type.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, Type.DefaultBinder, CallingConventions.Standard, methodInfoData.parameters, null));
                if (generics != null) cachedMethods[methodInfoData] = cachedMethods[methodInfoData].MakeGenericMethod(generics);
            }
            MethodInfo method = cachedMethods[methodInfoData];
            if (method != null)
            {
                return method.Invoke(type, parameters);
            }
            else
            {
                throw new InvalidOperationException("No method was found with the specified name and parameters: `" + methodName + "`, with the parameters: {" + parameters.Select(x => x.GetType().ToString()).Concat() + "}");
            }
        }

        private struct MethodInfoData
        {
            public readonly Type type;
            public readonly string name;
            public readonly Type[] generics;
            public readonly Type[] parameters;
            public MethodInfoData(Type type, string name, Type[] generics, params Type[] parameters)
            {
                this.type = type;
                this.name = name;
                this.generics = generics;
                this.parameters = parameters;
            }

            public override int GetHashCode()
            {
                var hc = 0;
                if (type is object)
                    hc = type.GetHashCode();
                if (name is object)
                    hc = ((hc << 5) + hc) ^ name.GetHashCode();
                if (generics is object)
                    for (int i = 0; i < generics.Length; i++)
                    {
                        hc = ((hc << 5) + hc) ^ generics[i]?.GetHashCode() ?? 0;
                    }
                if (parameters is object)
                    for (int i = 0;i < parameters.Length;i++)
                    {
                        hc = ((hc << 5) + hc) ^ parameters[i]?.GetHashCode() ?? 0;
                    }
                return hc;
            }
            public override bool Equals(object obj)
            {
                if (obj is MethodInfoData other)
                {
                    return type == other.type && name == other.name && Enumerable.SequenceEqual(parameters, other.parameters);
                }
                else
                {
                    return base.Equals(obj);
                }
            }
        }
    }
}
