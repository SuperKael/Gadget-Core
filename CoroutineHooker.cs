using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore
{
    /// <summary>
    /// A simple object that always persists in the scene, ideal for attaching coroutines to.
    /// </summary>
    [DisallowMultipleComponent]
    public class CoroutineHooker : MonoBehaviour
    {
        private static CoroutineHooker Singleton;
        private static List<Action> syncActions = new List<Action>();

        static CoroutineHooker()
        {
            Singleton = new GameObject("CoroutineHooker", typeof(CoroutineHooker)).GetComponent<CoroutineHooker>();
            DontDestroyOnLoad(Singleton.gameObject);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Start()
        {
            if (this != Singleton)
            {
                GadgetCore.CoreLogger.LogWarning("CoroutineHooker should only be applied to its singleton object!");
                Destroy(this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Update()
        {
            lock (syncActions)
            {
                foreach (Action action in syncActions)
                {
                    action();
                }
                syncActions.Clear();
            }
        }

        /// <summary>
        /// Starts a coroutine on the singleton object.
        /// </summary>
        public static new Coroutine StartCoroutine(IEnumerator routine)
        {
            return ((MonoBehaviour)Singleton).StartCoroutine(routine);
        }

        /// <summary>
        /// Starts a coroutine on the singleton object.
        /// </summary>
        public static new Coroutine StartCoroutine(string methodName)
        {
            return ((MonoBehaviour)Singleton).StartCoroutine(methodName);
        }

        /// <summary>
        /// Starts a coroutine on the singleton object.
        /// </summary>
        public static new Coroutine StartCoroutine(string methodName, [UnityEngine.Internal.DefaultValue("null")] object value)
        {
            return ((MonoBehaviour)Singleton).StartCoroutine(methodName, value);
        }

        /// <summary>
        /// Stops a coroutine on the singleton object.
        /// </summary>
        public static new void StopCoroutine(string methodName)
        {
            ((MonoBehaviour)Singleton).StopCoroutine(methodName);
        }

        /// <summary>
        /// Stops a coroutine on the singleton object.
        /// </summary>
        public static new void StopCoroutine(IEnumerator routine)
        {
            ((MonoBehaviour)Singleton).StopCoroutine(routine);
        }

        /// <summary>
        /// Stops a coroutine on the singleton object.
        /// </summary>
        public static new void StopCoroutine(Coroutine routine)
        {
            ((MonoBehaviour)Singleton).StopCoroutine(routine);
        }

        /// <summary>
        /// Schedules an action to be run synchronously on the main thread at the next available time.
        /// </summary>
        public static void ScheduleSyncAction(Action syncAction)
        {
            lock(syncActions)
            {
                syncActions.Add(syncAction);
            }
        }
    }
}
