using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace GadgetCore.Util
{
    /// <summary>
    /// Provides the extension method 'InvokeMethod' for easily invoking methods with Reflection.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Const value representative of the combination of the four standard <see cref="BindingFlags"/> uses for getting class members:
        /// BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
        /// </summary>
        public const BindingFlags ALL_BF = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        private static readonly Dictionary<MethodInfoData, MethodInfo> cachedMethods = new Dictionary<MethodInfoData, MethodInfo>();
        private static readonly Dictionary<FieldInfoData, FieldInfo> cachedFields = new Dictionary<FieldInfoData, FieldInfo>();
        private static readonly Dictionary<FieldInfoData, Delegate> cachedGetters = new Dictionary<FieldInfoData, Delegate>();
        private static readonly Dictionary<FieldInfoData, Delegate> cachedSetters = new Dictionary<FieldInfoData, Delegate>();

        /// <summary>
        /// Determines if this <see cref="MemberInfo"/> is static.
        /// </summary>
        public static bool IsStatic(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            return memberInfo is FieldInfo fi ? fi.IsStatic :
                   memberInfo is MethodBase mb ? mb.IsStatic :
                   memberInfo is PropertyInfo pi ? pi.GetGetMethod(true)?.IsStatic ?? pi.GetSetMethod(true).IsStatic :
                   memberInfo is EventInfo ei ? ei.GetAddMethod(true).IsStatic :
                   memberInfo is Type t ? t.IsAbstract && t.IsSealed :
                   throw new InvalidOperationException("Unrecognized Member Type");
        }

        /// <summary>
        /// Gets the Type of the value that would be returned by <see cref="GetValue(MemberInfo, object)"/> for this <see cref="MemberInfo"/>.
        /// Returns null if <see cref="GetValue(MemberInfo, object)"/> cannot function for this <see cref="MemberInfo"/>.
        /// </summary>
        public static Type GetGetType(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            return memberInfo is FieldInfo fi ? fi.FieldType :
                   memberInfo is PropertyInfo pi ? pi.GetGetMethod(true) != null ? pi.PropertyType : null :
                   memberInfo is MethodInfo mi ? mi.ReturnType :
                   null;
        }

        /// <summary>
        /// Gets the Type of the value that is required by <see cref="SetValue(MemberInfo, object, object)"/> for this <see cref="MemberInfo"/>.
        /// Returns null if <see cref="SetValue(MemberInfo, object, object)"/> cannot function for this <see cref="MemberInfo"/>.
        /// </summary>
        public static Type GetSetType(this MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            return memberInfo is FieldInfo fi ? fi.FieldType :
                   memberInfo is PropertyInfo pi ? pi.GetSetMethod(true) != null ? pi.PropertyType : null :
                   memberInfo is MethodBase mb && mb.GetParameters().Length == 1 ? mb.GetParameters()[0].ParameterType :
                   null;
        }

        /// <summary>
        /// Attempts to get a value from this <see cref="MemberInfo"/> from the given <paramref name="obj"/>.
        /// Can only work with Fields, Methods with a return value, and Properties with a getter. For methods, all parameters are passed their default values.
        /// If retrieving a value from the member is not possible, simply returns <paramref name="memberInfo"/>.
        /// </summary>
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            return memberInfo is FieldInfo fi ? fi.GetValue(obj) :
                   memberInfo is PropertyInfo pi && pi.GetGetMethod(true) != null ? pi.GetGetMethod(true).Invoke(obj, new object[0]) :
                   memberInfo is MethodInfo mi && mi.ReturnType != typeof(void) ? mi.Invoke(obj, mi.GetParameters().Select(x => !Convert.IsDBNull(x.DefaultValue) ? x.DefaultValue : x.ParameterType.IsValueType ? Activator.CreateInstance(x.ParameterType) : null).ToArray()) :
                   memberInfo;
        }

        /// <summary>
        /// Attempts to set a value to this <see cref="MemberInfo"/> from the given <paramref name="obj"/>.
        /// Can only work with Fields, Methods with a single parameter, and Properties with a setter.
        /// Returns a value indicating whether the assignment was possible.
        /// Throws an <see cref="ArgumentException"/> if value Type is not assignable to member Type.
        /// </summary>
        public static bool SetValue(this MemberInfo memberInfo, object obj, object value)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            if (memberInfo is FieldInfo fi)
            {
                if (value != null && !fi.FieldType.IsAssignableFrom(value.GetType())) throw new ArgumentException("Type of value " + value.GetType() + " cannot be assigned to Type of field " + fi.FieldType, nameof(value));
                fi.SetValue(obj, value);
                return true;
            }
            if (memberInfo is PropertyInfo pi)
            {
                if (value != null && !pi.PropertyType.IsAssignableFrom(value.GetType())) throw new ArgumentException("Type of value " + value.GetType() + " cannot be assigned to Type of property " + pi.PropertyType, nameof(value));
                MethodInfo setMethod = pi.GetSetMethod(true);
                if (setMethod != null)
                {
                    setMethod.Invoke(obj, new[] { value });
                    return true;
                }
            }
            if (memberInfo is MethodBase mb)
            {
                ParameterInfo[] methodParams = mb.GetParameters();
                if (methodParams.Length != 1) return false;
                if (value != null && !methodParams[0].ParameterType.IsAssignableFrom(value.GetType())) throw new ArgumentException("Type of value " + value.GetType() + " cannot be assigned to Type of parameter " + methodParams[0].ParameterType, nameof(value));
                mb.Invoke(obj, new[] { value });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateGetter(FieldInfo)"/>
        /// </summary>
        public static T GetValue<T>(this FieldInfo field, object obj)
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType) && !typeof(T).IsSubclassOf(field.FieldType)) throw new InvalidCastException("Cannot cast from " + field.FieldType + " to " + typeof(T));
            FieldInfoData fieldInfoData = new FieldInfoData(field.DeclaringType, field.Name);
            if (!cachedGetters.ContainsKey(fieldInfoData)) cachedGetters.Add(fieldInfoData, CreateGetter(field));
            return cachedGetters[fieldInfoData].DynamicInvoke(obj) is T value ? value : default;
        }

        /// <summary>
        /// Gets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateGetter(FieldInfo)"/>
        /// </summary>
        public static object GetValue(this FieldInfo field, object obj)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(field.DeclaringType, field.Name);
            if (!cachedGetters.ContainsKey(fieldInfoData)) cachedGetters.Add(fieldInfoData, CreateGetter(field));
            return cachedGetters[fieldInfoData].DynamicInvoke(obj);
        }

        /// <summary>
        /// Sets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateSetter(FieldInfo)"/>
        /// </summary>
        public static void SetValue(this FieldInfo field, object obj, object val)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(field.DeclaringType, field.Name);
            if (!cachedSetters.ContainsKey(fieldInfoData)) cachedSetters.Add(fieldInfoData, CreateSetter(field));
            cachedSetters[fieldInfoData].DynamicInvoke(obj, val);
        }

        /// <summary>
        /// Gets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="fieldName">The name of the field to read from.</param>
        /// <returns>The value contained within the field.</returns>
        public static T GetFieldValue<T>(this object type, string fieldName)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, ALL_BF));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                if (!cachedGetters.ContainsKey(fieldInfoData)) cachedGetters.Add(fieldInfoData, CreateGetter(field));
                return cachedGetters[fieldInfoData].DynamicInvoke(type) is T value ? value : default;
            }
            else
            {
                throw new InvalidOperationException("No field was found with the specified name: `" + fieldName + "`");
            }
        }

        /// <summary>
        /// Gets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to read from.</param>
        /// <param name="fieldName">The name of the field to read from.</param>
        /// <returns>The value contained within the field.</returns>
        public static object GetFieldValue(this object type, string fieldName)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, ALL_BF));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                if (!cachedGetters.ContainsKey(fieldInfoData)) cachedGetters.Add(fieldInfoData, CreateGetter(field));
                return cachedGetters[fieldInfoData].DynamicInvoke(type);
            }
            else
            {
                throw new InvalidOperationException("No field was found with the specified name: `" + fieldName + "`");
            }
        }

        /// <summary>
        /// Sets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to write to.</param>
        /// <param name="fieldName">The name of the field to write to.</param>
        /// <param name="value">The value to set to the field.</param>
        public static void SetFieldValue(this object type, string fieldName, object value)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, ALL_BF));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                if (!cachedSetters.ContainsKey(fieldInfoData)) cachedSetters.Add(fieldInfoData, CreateSetter(field));
                cachedSetters[fieldInfoData].DynamicInvoke(type, value);
            }
            else
            {
                throw new InvalidOperationException("No field was found with the specified name: `" + fieldName + "`");
            }
        }

        /// <summary>
        /// Invokes the method with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">The parameters to run the method with.</param>
        /// <returns>The value returned by the invoked method.</returns>
        public static T InvokeMethod<T>(this object type, string methodName, params object[] parameters)
        {
            return InvokeMethod(type, methodName, null, parameters) is T value ? value : default;
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
            return InvokeMethod(type, methodName, generics, parameters) is T value ? value : default;
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
                cachedMethods.Add(methodInfoData, type.GetType().GetMethod(methodName, ALL_BF, Type.DefaultBinder, CallingConventions.Standard, methodInfoData.parameters, null));
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

        /// <summary>
        /// Creates a getter for the given FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Delegate CreateGetter(this FieldInfo field)
        {
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, field.FieldType, new[] { field.DeclaringType }, field.Module, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return getterMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(field.DeclaringType, field.FieldType));
        }

        /// <summary>
        /// Creates a getter for the given FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Func<object, T> CreateGetter<T>(this FieldInfo field)
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType) && !typeof(T).IsSubclassOf(field.FieldType)) throw new InvalidOperationException("Cannot create getter: `" + field.FieldType + "` cannot be cast to `" + typeof(T) + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create getter: Value type `" + field.FieldType + "` can only be boxed to `object`");
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(T), new[] { typeof(object) }, field.Module, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldfld, field);
            }
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, typeof(T));
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, typeof(T));
                }
            }
            gen.Emit(OpCodes.Ret);
            return (Func<object, T>)getterMethod.CreateDelegate(typeof(Func<object, T>));
        }

        /// <summary>
        /// Creates a getter for the given FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Func<S, T> CreateGetter<S, T>(this FieldInfo field)
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType) && !typeof(T).IsSubclassOf(field.FieldType)) throw new InvalidOperationException("Cannot create getter: `" + field.FieldType + "` cannot be cast to `" + typeof(T) + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create getter: Value type `" + field.FieldType + "` can only be boxed to `object`");
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(T), new[] { typeof(S) }, field.Module, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, typeof(T));
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, typeof(T));
                }
            }
            gen.Emit(OpCodes.Ret);
            return (Func<S, T>)getterMethod.CreateDelegate(typeof(Func<S, T>));
        }

        /// <summary>
        /// Creates a getter for the given static FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Delegate CreateStaticGetter(this FieldInfo field)
        {
            if (!field.IsStatic) throw new InvalidOperationException("Cannot make a static getter for a non-static field!");
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, field.FieldType, new Type[] { }, field.Module, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldsfld, field);
            gen.Emit(OpCodes.Ret);
            return getterMethod.CreateDelegate(typeof(Func<>).MakeGenericType(field.FieldType));
        }

        /// <summary>
        /// Creates a getter for the given static FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Func<T> CreateStaticGetter<T>(this FieldInfo field)
        {
            if (!field.IsStatic) throw new InvalidOperationException("Cannot make a static getter for a non-static field!");
            if (!typeof(T).IsAssignableFrom(field.FieldType) && !typeof(T).IsSubclassOf(field.FieldType)) throw new InvalidOperationException("Cannot create getter: `" + field.FieldType + "` cannot be cast to `" + typeof(T) + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create getter: Value type `" + field.FieldType + "` can only be boxed to `object`");
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(T), new Type[] { }, field.Module, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldsfld, field);
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, typeof(T));
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, typeof(T));
                }
            }
            gen.Emit(OpCodes.Ret);
            return (Func<T>)getterMethod.CreateDelegate(typeof(Func<T>));
        }

        /// <summary>
        /// Creates a setter for the given FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Delegate CreateSetter(this FieldInfo field)
        {
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new[] { field.DeclaringType, field.FieldType }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return setterMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(field.DeclaringType, field.FieldType));
        }

        /// <summary>
        /// Creates a setter for the given FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Action<object, T> CreateSetter<T>(this FieldInfo field)
        {
            if (!field.FieldType.IsAssignableFrom(typeof(T)) && !field.FieldType.IsSubclassOf(typeof(T))) throw new InvalidOperationException("Cannot create setter: `" + typeof(T) + "` cannot be cast to `" + field.FieldType + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create setter: Value type `" + field.FieldType + "` can only be unboxed from `object`");
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new[] { typeof(object), typeof(T) }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldarg_1);
            }
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox, field.FieldType);
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, field.FieldType);
                }
            }
            gen.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            return (Action<object, T>)setterMethod.CreateDelegate(typeof(Action<object, T>));
        }

        /// <summary>
        /// Creates a setter for the given FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Action<S, T> CreateSetter<S, T>(this FieldInfo field)
        {
            if (!field.FieldType.IsAssignableFrom(typeof(T)) && !field.FieldType.IsSubclassOf(typeof(T))) throw new InvalidOperationException("Cannot create setter: `" + typeof(T) + "` cannot be cast to `" + field.FieldType + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create setter: Value type `" + field.FieldType + "` can only be unboxed from `object`");
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new[] { typeof(S), typeof(T) }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
            }
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox, field.FieldType);
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, field.FieldType);
                }
            }
            gen.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            return (Action<S, T>)setterMethod.CreateDelegate(typeof(Action<S, T>));
        }

        /// <summary>
        /// Creates a setter for the given static FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Delegate CreateStaticSetter(this FieldInfo field)
        {
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new[] { field.FieldType }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stsfld, field);
            gen.Emit(OpCodes.Ret);
            return setterMethod.CreateDelegate(typeof(Action<>).MakeGenericType(field.FieldType));
        }

        /// <summary>
        /// Creates a setter for the given static FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Action<T> CreateStaticSetter<T>(this FieldInfo field)
        {
            if (!field.FieldType.IsAssignableFrom(typeof(T)) && !field.FieldType.IsSubclassOf(typeof(T))) throw new InvalidOperationException("Cannot create setter: `" + typeof(T) + "` cannot be cast to `" + field.FieldType + "`");
            if (typeof(T) != field.FieldType && field.FieldType.IsValueType && typeof(T) != typeof(object)) throw new InvalidOperationException("Cannot create setter: Value type `" + field.FieldType + "` can only be unboxed from `object`");
            if (!field.IsStatic) throw new InvalidOperationException("Cannot make a static setter for a non-static field!");
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new[] { typeof(T) }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            if (typeof(T) != field.FieldType)
            {
                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox, field.FieldType);
                }
                else
                {
                    gen.Emit(OpCodes.Castclass, field.FieldType);
                }
            }
            gen.Emit(OpCodes.Stsfld, field);
            gen.Emit(OpCodes.Ret);
            return (Action<T>)setterMethod.CreateDelegate(typeof(Action<T>));
        }

        /// <summary>
        /// Creates an invoker delegate for the given MethodInfo, allowing you to repeatedly invoke it without repeated calls to MethodInfo's Invoke method.
        /// </summary>
        public static T CreateInvoker<T>(this MethodInfo method, object targetInstance = null) where T : Delegate
        {
            return (T)CreateInvoker(method, targetInstance);
        }

        /// <summary>
        /// Creates an invoker delegate for the given MethodInfo, allowing you to repeatedly invoke it without repeated calls to MethodInfo's Invoke method.
        /// </summary>
        public static Delegate CreateInvoker(this MethodInfo method, object targetInstance = null)
        {
            IEnumerable<Type> parameterTypes = method.GetParameters().Select(p => p.ParameterType);
            Type[] parameterTypesArray = parameterTypes.ToArray();
            Type delegateType;

            if (method.ReturnType == typeof(void))
            {
                delegateType = Expression.GetActionType(parameterTypesArray);
            }
            else
            {
                delegateType = Expression.GetFuncType(parameterTypes.Concat(new[] { method.ReturnType }).ToArray());
            }

            if (method.IsStatic || targetInstance != null)
            {
                return Delegate.CreateDelegate(delegateType, targetInstance, method);
            }
            else
            {
                DynamicMethod invokerMethod = new DynamicMethod(method.ReflectedType.FullName + ".invoke_" + method.Name,
                    MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, method.ReturnType, parameterTypesArray, method.Module, true);
                ILGenerator gen = invokerMethod.GetILGenerator();

                gen.Emit(OpCodes.Ldnull);
                for (int i = 0;i < parameterTypesArray.Length;i++)
                {
                    switch (i)
                    {
                        case 0:
                            gen.Emit(OpCodes.Ldarg_0);
                            break;
                        case 1:
                            gen.Emit(OpCodes.Ldarg_1);
                            break;
                        case 2:
                            gen.Emit(OpCodes.Ldarg_2);
                            break;
                        case 3:
                            gen.Emit(OpCodes.Ldarg_3);
                            break;
                        default:
                            gen.Emit(OpCodes.Ldarg, i);
                            break;
                    }
                }
                if (method.IsVirtual)
                {
                    gen.Emit(OpCodes.Callvirt, method);
                }
                else
                {
                    gen.Emit(OpCodes.Call, method);
                }
                gen.Emit(OpCodes.Ret);

                return invokerMethod.CreateDelegate(delegateType);
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
                    return type == other.type && name == other.name && parameters.SequenceEqual(other.parameters);
                }
                else
                {
                    return base.Equals(obj);
                }
            }
        }

        private struct FieldInfoData
        {
            public readonly Type type;
            public readonly string name;
            public FieldInfoData(Type type, string name)
            {
                this.type = type;
                this.name = name;
            }

            public override int GetHashCode()
            {
                var hc = 0;
                if (type is object)
                    hc = type.GetHashCode();
                if (name is object)
                    hc = ((hc << 5) + hc) ^ name.GetHashCode();
                return hc;
            }
            public override bool Equals(object obj)
            {
                if (obj is MethodInfoData other)
                {
                    return type == other.type && name == other.name;
                }
                else
                {
                    return base.Equals(obj);
                }
            }
        }
    }
}
