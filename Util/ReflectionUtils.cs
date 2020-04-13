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
            return (T)InvokeMethod(type, methodName, parameters);
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
            MethodInfoData methodInfoData = new MethodInfoData(type.GetType(), methodName, parameters.Select(x => x.GetType()).ToArray());
            if (!cachedMethods.ContainsKey(methodInfoData))
            {
                cachedMethods.Add(methodInfoData, type.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, Type.DefaultBinder, CallingConventions.Standard, methodInfoData.parameters, null));
            }
            MethodInfo method = cachedMethods[methodInfoData];
            if (method != null)
            {
                return method.Invoke(type, parameters);
            }
            else
            {
                throw new InvalidOperationException("No method was found with the specified name and parameters!");
            }
        }

        private struct MethodInfoData
        {
            public readonly Type type;
            public readonly string name;
            public readonly Type[] parameters;
            public MethodInfoData(Type type, string name, params Type[] parameters)
            {
                this.type = type;
                this.name = name;
                this.parameters = parameters;
            }

            public override int GetHashCode()
            {
                var hc = 0;
                if (!ReferenceEquals(type, null))
                    hc = type.GetHashCode();
                if (!ReferenceEquals(name, null))
                    hc = ((hc << 5) + hc) ^ name.GetHashCode();
                if (!ReferenceEquals(parameters, null))
                    for (int i = 0;i < parameters.Length;i++)
                    {
                        hc = ((hc << 5) + hc) ^ parameters[i].GetHashCode();
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
