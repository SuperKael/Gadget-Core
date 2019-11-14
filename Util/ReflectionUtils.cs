using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        public static object InvokeMethod(this object type, string methodName, params object[] parameters)
        {
            MethodInfoData methodInfoData = new MethodInfoData(type.GetType(), methodName, parameters.Select(x => x.GetType()).ToArray());
            if (!cachedMethods.ContainsKey(methodInfoData))
            {
                cachedMethods.Add(methodInfoData, type.GetType().GetMethod(methodName, methodInfoData.parameters));
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
        }
    }
}
