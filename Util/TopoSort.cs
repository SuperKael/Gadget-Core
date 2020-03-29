using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.Util
{
    /// <summary>
    /// Utility class for performing topological sorts.
    /// </summary>
    public static class TopoSort
    {
        /// <summary>
        /// Topologically sorts the given dictionary of values and dependencies using Kahn's algorithm. Returns null if there are any cyclic dependencies.
        /// </summary>
        /// <returns>Sorted values</returns>
        public static List<T> Sort<T>(IDictionary<T, IEnumerable<T>> values)
        {
            HashSet<T> nodes = new HashSet<T>(values.Keys);
            HashSet<Tuple<T, T>> edges = new HashSet<Tuple<T, T>>(values.SelectMany(x => x.Value.Select(y => Tuple.Create(x.Key, y))));
            List<T> L = new List<T>();
            HashSet<T> S = new HashSet<T>(nodes.Where(n => edges.All(e => !e.Item2.Equals(n))));
            while (S.Any())
            {
                T n = S.First();
                S.Remove(n);
                L.Add(n);
                foreach (Tuple<T, T> e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    T m = e.Item2;
                    edges.Remove(e);
                    if (edges.All(me => !me.Item2.Equals(m)))
                    {
                        S.Add(m);
                    }
                }
            }
            if (edges.Any())
            {
                return null;
            }
            else
            {
                return L;
            }
        }
    }
}
