using System;
using System.Collections.Generic;

namespace GadgetCore.Util
{
    /// <summary>
    /// Implementation of <see cref="IComparer{T}"/> that uses a <see cref="Comparison{T}"/>
    /// </summary>
    public class ComparisonComparer<T> : Comparer<T>, IEqualityComparer<T>
    {
        private Comparison<T> comparison;

        /// <summary>
        /// Creates a new <see cref="ComparisonComparer{T}"/>
        /// </summary>
        public ComparisonComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        /// <summary>
        /// Performs a comparison of two objects of the same type and returns a value indicating whether one object is less than, equal to, or greater than the other.
        /// </summary>
        public override int Compare(T x, T y)
        {
            return comparison(x, y);
        }

        /// <summary>
        /// Performs comparison of two objects of the same type and determines if they can be considered equal by the comparison.
        /// </summary>
        public bool Equals(T x, T y)
        {
            return comparison(x, y) == 0;
        }

        /// <summary>
        /// Calls T's GetHashCode.
        /// </summary>
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
