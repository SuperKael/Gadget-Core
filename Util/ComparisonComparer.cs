using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.Util
{
    /// <summary>
    /// Implementation of <see cref="IComparer{T}"/> that uses a <see cref="Comparison{T}"/>
    /// </summary>
    public class ComparisonComparer<T> : Comparer<T>
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
    }
}
