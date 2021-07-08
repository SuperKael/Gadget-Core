﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// Replaces one component with another on a <see cref="GameObject"/>
        /// </summary>
        public static NewComponent ReplaceComponent<OldComponent, NewComponent>(this GameObject obj) where OldComponent : MonoBehaviour where NewComponent : MonoBehaviour
        {
            OldComponent oldComponent = obj.GetComponent<OldComponent>();
            if (oldComponent == null) throw new InvalidOperationException("Object does not have a " + typeof(OldComponent).Name + " Component!");
            bool isEnabled = obj.activeSelf;
            if (isEnabled) obj.SetActive(false);
            NewComponent newComponent = obj.AddComponent<NewComponent>();
            FieldInfo[] fields = typeof(OldComponent).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo field in fields)
            {
                FieldInfo newField = typeof(NewComponent).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (newField != null && field.FieldType == newField.FieldType)
                {
                    newField.SetValue(newComponent, field.GetValue(oldComponent));
                }
            }
            UnityEngine.Object.Destroy(oldComponent);
            if (isEnabled) obj.SetActive(true);
            return newComponent;
        }

        /// <summary>
        /// Replaces one component with another on a <see cref="GameObject"/>
        /// </summary>
        public static MonoBehaviour ReplaceComponent(this GameObject obj, Type oldComponentType, Type newComponentType)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(oldComponentType)) throw new ArgumentException("oldComponentType must be a MonoBehaviour!");
            if (!typeof(MonoBehaviour).IsAssignableFrom(newComponentType)) throw new ArgumentException("newComponentType must be a MonoBehaviour!");
            MonoBehaviour oldComponent = obj.GetComponent(oldComponentType) as MonoBehaviour;
            if (oldComponent == null) throw new InvalidOperationException("Object does not have a " + oldComponentType.Name + " Component!");
            bool isEnabled = obj.activeSelf;
            if (isEnabled) obj.SetActive(false);
            MonoBehaviour newComponent = obj.AddComponent(newComponentType) as MonoBehaviour;
            FieldInfo[] fields = oldComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo field in fields)
            {
                FieldInfo newField = newComponentType.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (newField != null && field.FieldType == newField.FieldType)
                {
                    newField.SetValue(newComponent, field.GetValue(oldComponent));
                }
            }
            UnityEngine.Object.Destroy(oldComponent);
            if (isEnabled) obj.SetActive(true);
            return newComponent;
        }

        /// <summary>
        /// Concatenates the strings in the given <see cref="IEnumerable{T}"/> together. Uses ", " as the seperator if one is not explicitly specified.
        /// </summary>
        public static string Concat(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0 && seperator != null) a.Append(seperator); a.Append(b); return a; }).ToString();
        }

        /// <summary>
        /// Concatenates the objects in the given <see cref="IEnumerable{T}"/> together. Uses ", " as the seperator if one is not explicitly specified.
        /// </summary>
        public static string Concat<T>(this IEnumerable<T> list, Func<T, string> predicate, string seperator = ", ")
        {
            return list.Aggregate(new StringBuilder(), (a, b) => { if (a.Length > 0 && seperator != null) a.Append(seperator); a.Append(predicate(b)); return a; }).ToString();
        }

        /// <summary>
        /// Recursively concatenates the objects in the given <see cref="IEnumerable{T}"/> together. Uses ", " as the seperator if one is not explicitly specified.
        /// </summary>
        public static string RecursiveConcat<T>(this IEnumerable<T> list, Func<object, string> predicate = null, string seperator = ", ")
        {
            return list.Concat((x) => RecursiveConcatInternal(x, predicate ?? ((o) => o?.ToString() ?? "null"), seperator), seperator);
        }

        private static string RecursiveConcatInternal(object obj, Func<object, string> predicate, string seperator = ", ")
        {
            return obj is IEnumerable enumerable ? enumerable.Cast<object>().Concat((x) => RecursiveConcatInternal(x, predicate, seperator), seperator) : predicate(obj);
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
        /// Executes a given <see cref="Action"/> for each entry in the <see cref="IEnumerable{T}"/>
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> handler)
        {
            foreach (T item in source) handler(item);
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> for each entry in the <see cref="IEnumerable{T}"/>
        /// </summary>
        public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> source, Func<T, TResult> handler)
        {
            List<TResult> results = new List<TResult>();
            foreach (T item in source) results.Add(handler(item));
            return results;
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
