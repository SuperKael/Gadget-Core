using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace GadgetCore.Util
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0041 // Use 'is null' check
    public static class Tuple
    {
        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }

    [DebuggerDisplay("Item1={Item1}")]
    public class Tuple<T1> : IFormattable
    {
        public T1 Item1 { get; private set; }

        public Tuple(T1 item1)
        {
            Item1 = item1;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0}", Item1);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2}")]
    public class Tuple<T1, T2> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1}", Item1, Item2);
        }
    }


    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3}")]
    public class Tuple<T1, T2, T3> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2}", Item1, Item2, Item3);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3};Item4={Item4}")]
    public class Tuple<T1, T2, T3, T4> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;
        private static readonly IEqualityComparer<T4> Item4Comparer = EqualityComparer<T4>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            if (!ReferenceEquals(Item4, null))
                hc = ((hc << 5) + hc) ^ Item4Comparer.GetHashCode(Item4);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3, T4>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3) &&
                       Item4Comparer.Equals(Item4, other.Item4);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3}", Item1, Item2, Item3, Item4);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3};Item4={Item4};Item5={Item5}")]
    public class Tuple<T1, T2, T3, T4, T5> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;
        private static readonly IEqualityComparer<T4> Item4Comparer = EqualityComparer<T4>.Default;
        private static readonly IEqualityComparer<T5> Item5Comparer = EqualityComparer<T5>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            if (!ReferenceEquals(Item4, null))
                hc = ((hc << 5) + hc) ^ Item4Comparer.GetHashCode(Item4);
            if (!ReferenceEquals(Item5, null))
                hc = ((hc << 5) + hc) ^ Item5Comparer.GetHashCode(Item5);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3, T4, T5>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3) &&
                       Item4Comparer.Equals(Item4, other.Item4) &&
                       Item5Comparer.Equals(Item5, other.Item5);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3},{4}", Item1, Item2, Item3, Item4, Item5);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3};Item4={Item4};Item5={Item5};Item6={Item6}")]
    public class Tuple<T1, T2, T3, T4, T5, T6> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;
        private static readonly IEqualityComparer<T4> Item4Comparer = EqualityComparer<T4>.Default;
        private static readonly IEqualityComparer<T5> Item5Comparer = EqualityComparer<T5>.Default;
        private static readonly IEqualityComparer<T6> Item6Comparer = EqualityComparer<T6>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            if (!ReferenceEquals(Item4, null))
                hc = ((hc << 5) + hc) ^ Item4Comparer.GetHashCode(Item4);
            if (!ReferenceEquals(Item5, null))
                hc = ((hc << 5) + hc) ^ Item5Comparer.GetHashCode(Item5);
            if (!ReferenceEquals(Item6, null))
                hc = ((hc << 5) + hc) ^ Item6Comparer.GetHashCode(Item6);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3, T4, T5, T6>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3) &&
                       Item4Comparer.Equals(Item4, other.Item4) &&
                       Item5Comparer.Equals(Item5, other.Item5) &&
                       Item6Comparer.Equals(Item6, other.Item6);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3},{4},{5}", Item1, Item2, Item3, Item4, Item5, Item6);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3};Item4={Item4};Item5={Item5};Item6={Item6};Item7={Item7}")]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;
        private static readonly IEqualityComparer<T4> Item4Comparer = EqualityComparer<T4>.Default;
        private static readonly IEqualityComparer<T5> Item5Comparer = EqualityComparer<T5>.Default;
        private static readonly IEqualityComparer<T6> Item6Comparer = EqualityComparer<T6>.Default;
        private static readonly IEqualityComparer<T7> Item7Comparer = EqualityComparer<T7>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            if (!ReferenceEquals(Item4, null))
                hc = ((hc << 5) + hc) ^ Item4Comparer.GetHashCode(Item4);
            if (!ReferenceEquals(Item5, null))
                hc = ((hc << 5) + hc) ^ Item5Comparer.GetHashCode(Item5);
            if (!ReferenceEquals(Item6, null))
                hc = ((hc << 5) + hc) ^ Item6Comparer.GetHashCode(Item6);
            if (!ReferenceEquals(Item7, null))
                hc = ((hc << 5) + hc) ^ Item7Comparer.GetHashCode(Item7);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3, T4, T5, T6, T7>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3) &&
                       Item4Comparer.Equals(Item4, other.Item4) &&
                       Item5Comparer.Equals(Item5, other.Item5) &&
                       Item6Comparer.Equals(Item6, other.Item6) &&
                       Item7Comparer.Equals(Item7, other.Item7);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3},{4},{5},{6}", Item1, Item2, Item3, Item4, Item5, Item6, Item7);
        }
    }

    [DebuggerDisplay("Item1={Item1};Item2={Item2};Item3={Item3};Item4={Item4};Item5={Item5};Item6={Item6};Item7={Item7};Item8={Item8}")]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7, T8> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        public T4 Item4 { get; private set; }
        public T5 Item5 { get; private set; }
        public T6 Item6 { get; private set; }
        public T7 Item7 { get; private set; }
        public T8 Item8 { get; private set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;
        private static readonly IEqualityComparer<T3> Item3Comparer = EqualityComparer<T3>.Default;
        private static readonly IEqualityComparer<T4> Item4Comparer = EqualityComparer<T4>.Default;
        private static readonly IEqualityComparer<T5> Item5Comparer = EqualityComparer<T5>.Default;
        private static readonly IEqualityComparer<T6> Item6Comparer = EqualityComparer<T6>.Default;
        private static readonly IEqualityComparer<T7> Item7Comparer = EqualityComparer<T7>.Default;
        private static readonly IEqualityComparer<T8> Item8Comparer = EqualityComparer<T8>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = ((hc << 5) + hc) ^ Item2Comparer.GetHashCode(Item2);
            if (!ReferenceEquals(Item3, null))
                hc = ((hc << 5) + hc) ^ Item3Comparer.GetHashCode(Item3);
            if (!ReferenceEquals(Item4, null))
                hc = ((hc << 5) + hc) ^ Item4Comparer.GetHashCode(Item4);
            if (!ReferenceEquals(Item5, null))
                hc = ((hc << 5) + hc) ^ Item5Comparer.GetHashCode(Item5);
            if (!ReferenceEquals(Item6, null))
                hc = ((hc << 5) + hc) ^ Item6Comparer.GetHashCode(Item6);
            if (!ReferenceEquals(Item7, null))
                hc = ((hc << 5) + hc) ^ Item7Comparer.GetHashCode(Item7);
            if (!ReferenceEquals(Item8, null))
                hc = ((hc << 5) + hc) ^ Item8Comparer.GetHashCode(Item8);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2, T3, T4, T5, T6, T7, T8>;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) &&
                       Item2Comparer.Equals(Item2, other.Item2) &&
                       Item3Comparer.Equals(Item3, other.Item3) &&
                       Item4Comparer.Equals(Item4, other.Item4) &&
                       Item5Comparer.Equals(Item5, other.Item5) &&
                       Item6Comparer.Equals(Item6, other.Item6) &&
                       Item7Comparer.Equals(Item7, other.Item7) &&
                       Item8Comparer.Equals(Item8, other.Item8);
        }

        public override string ToString() { return ToString(null, CultureInfo.CurrentCulture); }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1},{2},{3},{4},{5},{6},{7}", Item1, Item2, Item3, Item4, Item5, Item6, Item7, Item8);
        }
    }
#pragma warning restore IDE0041 // Use 'is null' check
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}