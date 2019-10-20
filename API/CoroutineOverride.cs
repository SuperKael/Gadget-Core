using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace API.GadgetCore
{
    /// <summary>
    /// Extend this class and override Next(int PC, T instance, ref object[] objs, ref object[] parameters) to patch Coroutine methods.
    /// (Coroutines are methods that can delay their own execution using yield instructions, although if you didn't already know that you probably shouldn't try to override one.)
    /// To override a Coroutine method with this, patch the Prefix of the method, and make sure to ref the __result param. Set __result to an instance of your override class, and return false from the Prefix.
    /// For implementation details of the Next method, check the documentation there.
    /// </summary>
    public abstract class CoroutineOverride<T> : IEnumerator, IDisposable, IEnumerator<object> where T : MonoBehaviour
    {
        public const BindingFlags InstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags StaticFlags = BindingFlags.NonPublic | BindingFlags.Static;

        object IEnumerator.Current
        {
            get
            {
                return current;
            }
        }

        object IEnumerator<object>.Current
        {
            get
            {
                return current;
            }
        }

        private YieldInstruction current;
        private bool disposing;
        private int PC;

        private T instance;
        private object[] objs;
        private FieldInfo[] parameters;

        /// <summary>
        /// <para><paramref name="instance"/>: an instance of the calling class, as provided by the __instance parameter in a harmony patch method.</para>
        /// <para><paramref name="objs"/>: an array of objects you want to provide to your Coroutine override, perhaps the parameters of the Coroutine being overidden.</para>
        /// <para><paramref name="parameters"/>: an array of FieldInfo representing private fields you want to give your Coroutine access to. A FieldInfo is obtained using: <code>typeof(ClassType).GetField("FieldName", CoroutineOverride.InstanceFlags OR CoroutineOverride.StaticFlags)</code></para>
        /// </summary>
        public CoroutineOverride(T instance, object[] objs, params FieldInfo[] parameters)
        {
            this.instance = instance;
            this.objs = objs;
            this.parameters = parameters;
        }

        /// <summary>
        /// <para><paramref name="instance"/>: an instance of the calling class, as provided by the __instance parameter in a harmony patch method.</para>
        /// <para><paramref name="parameters"/>: an array of FieldInfo representing private fields you want to give your Coroutine access to. Obtained using: <code>typeof(ClassType).GetField("FieldName", CoroutineOverride.InstanceFlags OR CoroutineOverride.StaticFlags)</code></para>
        /// </summary>
        public CoroutineOverride(T instance, params FieldInfo[] parameters)
        {
            this.instance = instance;
            this.parameters = parameters;
        }

        /// <summary>
        /// Override to provide the code to use for overriding the Coroutine.
        /// Unfortunately, due to various limitations, the code here has to be written a bit differently than you would a traditional coroutine.
        /// This method will get called repeatedly. The first time it is called, PC will be 0. Each additional time that it is called, PC will have been incremented by 1.
        /// You should switch on the value of PC to provide different code to run depending on the state. Return a YieldInstruction, (I.E. WaitForSeconds), when you are done with the current state.
        /// Return null to specify that your Coroutine is done executing. (Equivalent to 'yield break' in an actual Coroutine). You do not need to ever return null to stop, however if the Coroutine you are overriding does, you probably should too.
        /// Bear in mind that at any time the caller of the Coroutine may decide to stop it, in which case Next will stop getting called with no warning. To create a loop, simply using the modulus operator (%) on PC in your switch.
        /// <para><paramref name="objs"/>: an array of objects matching the objects your Coroutine was given in its constructor. Will be null if no objs were given in the Coroutine's constructor. Changes to these values will persist to later invocations of Next.</para>
        /// <para><paramref name="parameters"/>: an array of objects matching the private fields your Coroutine was given access to in its constructor. Can have their values set to change the fields they reference.</para>
        /// </summary>
        public abstract YieldInstruction Next(int PC, T instance, ref object[] objs, ref object[] parameters);

        /// <summary>
        /// Part of IEnumerator implementation. You should never invoke this yourself, as this is meant to only be used by the UnityEngine.
        /// </summary>
        public bool MoveNext()
        {
            if (PC >= 0)
            {
                object[] paramValues = new object[parameters.Length];
                for (int i = 0; i < paramValues.Length; i++) paramValues[i] = parameters[i].GetValue(instance);
                YieldInstruction insn = Next(PC, instance, ref objs, ref paramValues);
                for (int i = 0; i < paramValues.Length; i++) parameters[i].SetValue(instance, paramValues[i]);
                if (insn != null)
                {
                    current = insn;
                }
                else
                {
                    return false;
                }
                if (!disposing)
                {
                    PC++;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Part of IEnumerator implementation. You should never invoke this, as it will always throw a NotSupportedException.
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Part of IDisposable implementation. You should never invoke this yourself, as this is meant to only be used by the UnityEngine.
        /// </summary>
        public void Dispose()
        {
            disposing = true;
            PC = -1;
        }
    }
}
