using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace GadgetCore.Util
{
    /// <summary>
    /// Uses a dedicated thread to watch for changes in the value of a field.
    /// </summary>
    /// <typeparam name="T">The type of the value being watched</typeparam>
    public class ThreadedWatcher<T>
    {
        private readonly object container;
        private readonly Func<object, T> getter;
        private readonly Func<T, T, bool> listener;
        private readonly int sleepInterval;
        private readonly EqualityComparer<T> comparer;
        private readonly Func<Exception, bool> exceptionHandler;
        private bool live;
        private T val;

        /// <summary>
        /// Constructs and initiates a new <see cref="ThreadedWatcher{T}"/>. Use <see cref="Kill"/>, or return false from the <paramref name="listener"/>, to stop.
        /// </summary>
        public ThreadedWatcher(FieldInfo field, object container, Func<T, T, bool> listener, int sleepInterval = 10, EqualityComparer<T> comparer = null, Func<Exception, bool> exceptionHandler = null)
        {
            if (typeof(T).IsAssignableFrom(field.FieldType)) throw new InvalidOperationException("ThreadedWatcher type T must match the Type of the field being watched!");
            if (!field.IsStatic)
            {
                if (container == null) throw new ArgumentNullException(nameof(container), "ThreadedWatcher container must not be null for non-static fields!");
                if (container.GetType() != field.DeclaringType) throw new ArgumentException("ThreadedWatcher container must of the declaring type of the field being watched!", nameof(container));
            }
            getter = field.CreateGetter<T>();
            this.container = container;
            this.listener = listener;
            this.sleepInterval = sleepInterval;
            if (comparer != null) this.comparer = comparer;
            else this.comparer = EqualityComparer<T>.Default;
            this.exceptionHandler = exceptionHandler;
            val = getter(container);
            live = true;

            if (exceptionHandler != null)
                new Thread(WatchSafely).Start();
            else
                new Thread(Watch).Start();
        }

        /// <summary>
        /// Constructs and initiates a new <see cref="ThreadedWatcher{T}"/>. Use <see cref="Kill"/>, or return false from the <paramref name="listener"/>, to stop.
        /// </summary>
        public ThreadedWatcher(Func<object, T> getter, object container, Func<T, T, bool> listener, int sleepInterval = 10, EqualityComparer<T> comparer = null, Func<Exception, bool> exceptionHandler = null)
        {
            this.container = container;
            this.getter = getter;
            this.listener = listener;
            this.sleepInterval = sleepInterval;
            if (comparer != null) this.comparer = comparer;
            else this.comparer = EqualityComparer<T>.Default;
            this.exceptionHandler = exceptionHandler;
            val = getter(container);
            live = true;

            if (exceptionHandler != null)
                new Thread(WatchSafely).Start();
            else
                new Thread(Watch).Start();
        }

        /// <summary>
        /// Constructs and initiates a new <see cref="ThreadedWatcher{T}"/>. Use <see cref="Kill"/>, or return false from the <paramref name="listener"/>, to stop.
        /// </summary>
        public ThreadedWatcher(Func<T> getter, Func<T, T, bool> listener, int sleepInterval = 10, EqualityComparer<T> comparer = null, Func<Exception, bool> exceptionHandler = null)
        {
            container = null;
            this.getter = (o) => getter();
            this.listener = listener;
            this.sleepInterval = sleepInterval;
            if (comparer != null) this.comparer = comparer;
            else this.comparer = EqualityComparer<T>.Default;
            this.exceptionHandler = exceptionHandler;
            val = getter();
            live = true;

            if (exceptionHandler != null)
                new Thread(WatchSafely).Start();
            else
                new Thread(Watch).Start();
        }

        /// <summary>
        /// Permanently kills this watcher.
        /// </summary>
        public void Kill()
        {
            live = false;
        }

        /// <summary>
        /// Returns whether this watcher is currently live.
        /// </summary>
        public bool IsLive()
        {
            return live;
        }

        private void Watch()
        {
            T oldVal;
            do
            {
                oldVal = val;
                Thread.Sleep(sleepInterval);
                val = getter(container);
            }
            while (live && (comparer.Equals(oldVal, val) || listener(oldVal, val)));
        }

        private void WatchSafely()
        {
            Exception e;
            do
            {
                e = null;
                try
                {
                    T oldVal;
                    do
                    {
                        oldVal = val;
                        Thread.Sleep(sleepInterval);
                        val = getter(container);
                    }
                    while (live && (comparer.Equals(oldVal, val) || listener(oldVal, val)));
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }
            while ((e == null || exceptionHandler(e)) && live);
        }
    }
}
