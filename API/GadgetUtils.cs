using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Some general-purpose utility methods.
    /// </summary>
    public static class GadgetUtils
    {
        /// <summary>
        /// WaitAndInvoke is a coroutine that will wait for the specified timeout or condition, and then invoke the given method.
        /// </summary>
        /// <param name="method">The MethodBase to invoke.</param>
        /// <param name="timeout">The maximum time, in seconds, to wait.</param>
        /// <param name="condition">The condition on which to stop waiting early.</param>
        /// <param name="invokeInstance">The object to invoke the method on. May be null if the given method is static.</param>
        /// <param name="parameters">The parameters to pass to the invoked method.</param>
        public static IEnumerator WaitAndInvoke(MethodBase method, float timeout, Func<bool> condition = null, object invokeInstance = null, params object[] parameters)
        {
            if (condition == null) condition = () => false;
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup > (startTime + timeout) || condition());
            method.Invoke(invokeInstance, parameters);
        }
    }
}
