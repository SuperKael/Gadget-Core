using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace GadgetCore.Util
{
    /// <summary>
    /// Provides miscellaneous extension methods.
    /// </summary>
    public static class ExtensionUtils
    {
        /// <summary>
        /// Concatenates the strings in the given <see cref="IEnumerable{T}"/> together. Uses ", " as the seperator if one is not explicitly specified.
        /// </summary>
        public static string Concat(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0 && seperator != null) a.Append(seperator); a.Append(b); return a; }).ToString();
        }

        /// <summary>
        /// Finds the index of the given predicate, returns -1 if it is not found.
        /// </summary>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int index = 0;
            foreach (TSource item in source)
            {
                if (predicate.Invoke(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }


        /// <summary>
        /// Checks if the list has the given sublist at the specified index.
        /// </summary>
        public static bool IsSublist<T>(this List<T> list, List<T> sublist, int index, EqualityComparison<T> comparer = null)
        {
            if (comparer == null) comparer = (x, y) => Comparer<T>.Default.Compare(x, y) == 0;
            if (!comparer(list[index + sublist.Count - 1], sublist[sublist.Count - 1])) return false;
            int count = 0;
            while (count < sublist.Count - 1 && comparer(list[index + count], sublist[count]))
                count++;
            if (count == sublist.Count - 1)
                return true;
            return false;
        }

        /// <summary>
        /// Finds the index of the first instance of a sublist within a list.
        /// </summary>
        public static int IndexOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T> comparer = null)
        {
            if (start < 0) start = 0;
            if (comparer == null) comparer = (x, y) => Comparer<T>.Default.Compare(x, y) == 0;
            for (int index = start; index < list.Count - sublist.Count + 1; index++)
            {
                if (IsSublist(list, sublist, index, comparer))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Finds the index of the last instance of a sublist within a list.
        /// </summary>
        public static int LastIndexOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T> comparer = null)
        {
            if (start < 0) start = list.Count - sublist.Count - 1;
            if (comparer == null) comparer = (x, y) => Comparer<T>.Default.Compare(x, y) == 0;
            for (int index = start; index > 0; index--)
            {
                if (IsSublist(list, sublist, index, comparer))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Finds the indexes of all instances of the sublist within a list.
        /// </summary>
        public static int[] AllIndexesOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T> comparer = null)
        {
            if (start < 0) start = 0;
            if (comparer == null) comparer = (x, y) => Comparer<T>.Default.Compare(x, y) == 0;
            List<int> indexes = new List<int>();
            for (int index = start; index < list.Count - sublist.Count + 1; index++)
            {
                if (IsSublist(list, sublist, index, comparer))
                    indexes.Add(index);
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// Checks if the list has the given sublist at the specified index.
        /// </summary>
        public static bool IsSublist<T>(this List<T> list, List<T> sublist, int index, EqualityComparison<T>[] comparers = null)
        {
            if (comparers == null) comparers = Enumerable.Repeat<EqualityComparison<T>>((x, y) => Comparer<T>.Default.Compare(x, y) == 0, sublist.Count).ToArray();
            else if (comparers.Length != sublist.Count) throw new InvalidOperationException("comparers array must be of the same length as the sublist!");
            if (!comparers[sublist.Count - 1](list[index + sublist.Count - 1], sublist[sublist.Count - 1])) return false;
            int count = 0;
            while (count < sublist.Count - 1 && comparers[count](list[index + count], sublist[count]))
                count++;
            if (count == sublist.Count - 1)
                return true;
            return false;
        }

        /// <summary>
        /// Finds the index of the first instance of a sublist within a list.
        /// </summary>
        public static int IndexOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T>[] comparers = null)
        {
            if (start < 0) start = 0;
            if (comparers == null) comparers = Enumerable.Repeat<EqualityComparison<T>>((x, y) => Comparer<T>.Default.Compare(x, y) == 0, sublist.Count).ToArray();
            else if (comparers.Length != sublist.Count) throw new InvalidOperationException("comparers array must be of the same length as the sublist!");
            for (int index = start; index < list.Count - sublist.Count + 1; index++)
            {
                if (IsSublist(list, sublist, index, comparers))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Finds the index of the last instance of a sublist within a list.
        /// </summary>
        public static int LastIndexOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T>[] comparers = null)
        {
            if (start < 0) start = list.Count - sublist.Count - 1;
            if (comparers == null) comparers = Enumerable.Repeat<EqualityComparison<T>>((x, y) => Comparer<T>.Default.Compare(x, y) == 0, sublist.Count).ToArray();
            else if (comparers.Length != sublist.Count) throw new InvalidOperationException("comparers array must be of the same length as the sublist!");
            for (int index = start; index > 0; index--)
            {
                if (IsSublist(list, sublist, index, comparers))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Finds the indexes of all instances of the sublist within a list.
        /// </summary>
        public static int[] AllIndexesOfSublist<T>(this List<T> list, List<T> sublist, int start = 0, EqualityComparison<T>[] comparers = null)
        {
            if (start < 0) start = 0;
            if (comparers == null) comparers = Enumerable.Repeat<EqualityComparison<T>>((x, y) => Comparer<T>.Default.Compare(x, y) == 0, sublist.Count).ToArray();
            else if (comparers.Length != sublist.Count) throw new InvalidOperationException("comparers array must be of the same length as the sublist!");
            List<int> indexes = new List<int>();
            for (int index = start; index < list.Count - sublist.Count + 1; index++)
            {
                if (IsSublist(list, sublist, index, comparers))
                    indexes.Add(index);
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// Returns the highest parent object for this GameObject
        /// </summary>
        public static GameObject GetHighestParent(this GameObject obj)
        {
            return GetHighestParent(obj.transform).gameObject;
        }

        /// <summary>
        /// Returns the highest parent transform for this Transform
        /// </summary>
        public static Transform GetHighestParent(this Transform t)
        {
            while (t.parent != null) t = t.parent;
            return t;
        }

        /// <summary>
        /// Starts a coroutine after a delay
        /// </summary>
        public static Coroutine StartCoroutineDelayed(this MonoBehaviour script, IEnumerator routine, float delay)
        {
            return script.StartCoroutine(AfterDelay(routine, script, delay));
        }

        private static IEnumerator AfterDelay(IEnumerator routine, MonoBehaviour script, float delay)
        {
            yield return new WaitForSeconds(delay);
            yield return script.StartCoroutine(routine);
        }
    }

    /// <summary>
    /// A <see cref="Comparison{T}"/> that only checks for equality.
    /// </summary>
    public delegate bool EqualityComparison<T>(T x, T y);
}
