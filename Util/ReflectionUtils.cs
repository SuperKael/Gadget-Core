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
        private static Dictionary<MethodInfoData, MethodInfo> cachedMethods = new Dictionary<MethodInfoData, MethodInfo>();
        private static Dictionary<FieldInfoData, FieldInfo> cachedFields = new Dictionary<FieldInfoData, FieldInfo>();
        private static Dictionary<FieldInfo, Delegate> cachedGetters = new Dictionary<FieldInfo, Delegate>();
        private static Dictionary<FieldInfo, Delegate> cachedSetters = new Dictionary<FieldInfo, Delegate>();

        /// <summary>
        /// Gets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateGetter(FieldInfo)"/>
        /// </summary>
        public static T GetValue<T>(this FieldInfo field, object obj)
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType)) throw new InvalidCastException("Cannot cast from " + field.FieldType + " to " + typeof(T));
            if (!cachedGetters.ContainsKey(field)) cachedGetters.Add(field, CreateGetter(field));
            return cachedGetters[field].DynamicInvoke(obj) is T value ? value : default;
        }

        /// <summary>
        /// Gets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateGetter(FieldInfo)"/>
        /// </summary>
        public static object GetValue(this FieldInfo field, object obj)
        {
            if (!cachedGetters.ContainsKey(field)) cachedGetters.Add(field, CreateGetter(field));
            return cachedGetters[field].DynamicInvoke(obj);
        }

        /// <summary>
        /// Sets the value of this <see cref="FieldInfo"/>. Uses caching with <see cref="CreateSetter(FieldInfo)"/>
        /// </summary>
        public static void SetValue(this FieldInfo field, object obj, object val)
        {
            if (!cachedSetters.ContainsKey(field)) cachedSetters.Add(field, CreateSetter(field));
            cachedSetters[field].DynamicInvoke(obj, val);
        }

        /// <summary>
        /// Gets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="fieldName">The name of the field to invoke.</param>
        /// <returns>The value contained within the field.</returns>
        public static T GetFieldValue<T>(this object type, string fieldName)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                return GetValue<T>(field, type);
            }
            else
            {
                throw new InvalidOperationException("No field was found with the specified name: `" + fieldName + "`");
            }
        }

        /// <summary>
        /// Gets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="fieldName">The name of the field to invoke.</param>
        /// <returns>The value contained within the field.</returns>
        public static object GetFieldValue(this object type, string fieldName)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                return GetValue(field, type);
            }
            else
            {
                throw new InvalidOperationException("No field was found with the specified name: `" + fieldName + "`");
            }
        }

        /// <summary>
        /// Gets the value of a field with the specified name and parameters.
        /// </summary>
        /// <param name="type">The object instance to invoke upon.</param>
        /// <param name="fieldName">The name of the field to invoke.</param>
        /// <param name="value">The value to set to the field.</param>
        public static void SetFieldValue(this object type, string fieldName, object value)
        {
            FieldInfoData fieldInfoData = new FieldInfoData(type.GetType(), fieldName);
            if (!cachedFields.ContainsKey(fieldInfoData))
            {
                cachedFields.Add(fieldInfoData, type.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            }
            FieldInfo field = cachedFields[fieldInfoData];
            if (field != null)
            {
                SetValue(field, type, value);
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
            Func<Type[], Type> getType;
            IEnumerable<Type> types = method.GetParameters().Select(p => p.ParameterType);

            if (method.ReturnType == typeof(void))
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new Type[] { method.ReturnType });
            }

            Type delegateType = getType(types.ToArray());

            return Delegate.CreateDelegate(delegateType, targetInstance, method);
        }

        /// <summary>
        /// Creates a getter for the given FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Delegate CreateGetter(this FieldInfo field)
        {
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, field.FieldType, new Type[] { field.DeclaringType }, field.Module, true);
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
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(T), new Type[] { typeof(object) }, field.Module, true);
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
            gen.Emit(OpCodes.Ret);
            return (Func<object, T>)getterMethod.CreateDelegate(typeof(Func<object, T>));
        }

        /// <summary>
        /// Creates a getter for the given FieldInfo, allowing you to repeatedly get its value without repeated calls to GetValue
        /// </summary>
        public static Func<S, T> CreateGetter<S, T>(this FieldInfo field)
        {
            DynamicMethod getterMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(T), new Type[] { typeof(S) }, field.Module, true);
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
            return (Func<S, T>)getterMethod.CreateDelegate(typeof(Func<S, T>));
        }

        /// <summary>
        /// Creates a setter for the given FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Delegate CreateSetter(this FieldInfo field)
        {
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[] { field.DeclaringType, field.FieldType }, field.Module, true);
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
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[] { typeof(object), typeof(T) }, field.Module, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Action<object, T>)setterMethod.CreateDelegate(typeof(Action<object, T>));
        }

        /// <summary>
        /// Creates a setter for the given FieldInfo, allowing you to repeatedly set its value without repeated calls to SetValue
        /// </summary>
        public static Action<S, T> CreateSetter<S, T>(this FieldInfo field)
        {
            DynamicMethod setterMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[] { typeof(S), typeof(T) }, field.Module, true);
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
            return (Action<S, T>)setterMethod.CreateDelegate(typeof(Action<S, T>));
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
