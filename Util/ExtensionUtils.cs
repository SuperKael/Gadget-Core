using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
