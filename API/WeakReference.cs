using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a weak reference, which references an object while still allowing that object to be reclaimed by garbage collection.
    /// </summary>
    public sealed class WeakReference<T> : WeakReference
    {
        /// <summary>
        /// Initializes a new instance of the WeakReference class, referencing the specified object.
        /// </summary>
        /// <param name="target">The object to track or null.</param>
        public WeakReference(T target) : base(target) { }
        /// <summary>
        /// Initializes a new instance of the WeakReference class, referencing the specified object and using the specified resurrection tracking.
        /// </summary>
        /// <param name="target">An object to track.</param>
        /// <param name="trackResurrection">Indicates when to stop tracking the object. If true, the object is tracked after finalization; if false, the object is only tracked until finalization.</param>
        public WeakReference(T target, bool trackResurrection) : base(target, trackResurrection) { }
        /// <summary>
        /// Gets the object (the target) referenced by the current WeakReference object.
        /// </summary>
        /// <returns>null if the object referenced by the current System.WeakReference object has been garbage collected; otherwise, a reference to the object referenced by the current System.WeakReference object.</returns>
        /// <exception cref="System.InvalidOperationException">The reference to the target object is invalid. This exception can be thrown while setting this property if the value is a null reference.</exception>
        public T GetTarget()
        {
            return (T)Target;
        }
    }
}
