using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Provides a few simple convenience methods related to math.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Returns true if value is a perfect square. Otherwise returns false.
        /// </summary>
        public static bool IsPerfectSquare(int value)
        {
            return Mathf.Sqrt(value) % 1 == 0;
        }
        /// <summary>
        /// Returns the next perfect square after value. Value does not have to be a perfect square itself.
        /// </summary>
        public static int NextPerfectSquare(int value)
        {
            int result = Mathf.CeilToInt(Mathf.Sqrt(value + 1));
            return result * result;
        }
        /// <summary>
        /// Returns the smallest perfect square that is at least value. If value is already a perfect square, returns value.
        /// </summary>
        public static int SmallestPerfectSquare(int value)
        {
            int result = Mathf.CeilToInt(Mathf.Sqrt(value));
            return result * result;
        }
    }
}
