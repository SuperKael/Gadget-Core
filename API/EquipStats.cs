using System;
using System.Linq;

namespace GadgetCore.API
{
    /// <summary>
    /// Represents a set of all six of the player stats, intended to be used for custom equipment.
    /// </summary>
    public struct EquipStats
    {
        /// <summary>
        /// EquipStats where all stats are 0.
        /// </summary>
        public static readonly EquipStats NONE = new EquipStats(0, 0, 0, 0, 0, 0);
        /// <summary>
        /// EquipStats where all stats are 1.
        /// </summary>
        public static readonly EquipStats ONE = new EquipStats(1, 1, 1, 1, 1, 1);

        private readonly int[] stats;
        /// <summary>
        /// The VIT stat.
        /// </summary>
        public int VIT { get { return stats != null ? stats[0] : 0; } }
        /// <summary>
        /// The STR stat.
        /// </summary>
        public int STR { get { return stats != null ? stats[1] : 0; } }
        /// <summary>
        /// The DEX stat.
        /// </summary>
        public int DEX { get { return stats != null ? stats[2] : 0; } }
        /// <summary>
        /// The TEC stat.
        /// </summary>
        public int TEC { get { return stats != null ? stats[3] : 0; } }
        /// <summary>
        /// The MAG stat.
        /// </summary>
        public int MAG { get { return stats != null ? stats[4] : 0; } }
        /// <summary>
        /// The FTH stat.
        /// </summary>
        public int FTH { get { return stats != null ? stats[5] : 0; } }

        /// <summary>
        /// Creates a new instance of EquipStats that has the same stats as the given EquipStats.
        /// </summary>
        public EquipStats(EquipStats stats)
        {
            this.stats = stats.stats;
        }

        /// <summary>
        /// Creates a new instance of EquipStats that has the same stats as the given EquipStatsDouble, with the values truncated to integers.
        /// </summary>
        public static explicit operator EquipStats(EquipStatsDouble stats)
        {
            return new EquipStats((int)stats.VIT, (int)stats.STR, (int)stats.DEX, (int)stats.TEC, (int)stats.MAG, (int)stats.FTH);
        }

        /// <summary>
        /// Creates a new instance of EquipStats that has the specified stats.
        /// </summary>
        public EquipStats(int VIT, int STR, int DEX, int TEC, int MAG, int FTH)
        {
            stats = new int[6];
            stats[0] = VIT;
            stats[1] = STR;
            stats[2] = DEX;
            stats[3] = TEC;
            stats[4] = MAG;
            stats[5] = FTH;
        }

        /// <summary>
        /// Creates a new instance of EquipStats where all stats are the given value.
        /// </summary>
        public EquipStats(int stat)
        {
            stats = new int[6];
            stats[0] = stat;
            stats[1] = stat;
            stats[2] = stat;
            stats[3] = stat;
            stats[4] = stat;
            stats[5] = stat;
        }

        /// <summary>
        /// Creates a new instance of EquipStats from an array of stat values. The array must be of length 6.
        /// </summary>
        public EquipStats(int[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            stats = (int[])statArray.Clone();
        }

        /// <summary>
        /// Returns the stats as an array of values. Will be of length 6.
        /// </summary>
        public int[] GetStatArray()
        {
            return stats != null ? (int[])stats.Clone() : new int[6];
        }

        /// <summary>
        /// Gets the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public readonly int GetByIndex(int index)
        {
            return stats[index];
        }

        /// <summary>
        /// Sets the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public void SetByIndex(int index, int stat)
        {
            stats[index] = stat;
        }

        /// <summary>
        /// Adds to the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public void AddByIndex(int index, int stat)
        {
            stats[index] += stat;
        }

        /// <summary>
        /// Adds this EquipStat's values to the given stat value array. The array must be of length 6.
        /// </summary>
        public int[] AddTo(int[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            statArray[0] += VIT;
            statArray[1] += STR;
            statArray[2] += DEX;
            statArray[3] += TEC;
            statArray[4] += MAG;
            statArray[5] += FTH;
            return statArray;
        }

        /// <summary>
        /// Subtracts this EquipStat's values from the given stat value array. The array must be of length 6.
        /// </summary>
        public int[] SubtractFrom(int[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            statArray[0] -= VIT;
            statArray[1] -= STR;
            statArray[2] -= DEX;
            statArray[3] -= TEC;
            statArray[4] -= MAG;
            statArray[5] -= FTH;
            return statArray;
        }

        /// <summary>
        /// Determines if this EquipStats and the given EquipStats have the same stat values.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is EquipStats stats ? this == stats : base.Equals(obj);
        }

        /// <summary>
        /// Gets a hash code for this EquipStats.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = VIT.GetHashCode();
                hash = 31 * hash + STR.GetHashCode();
                hash = 31 * hash + DEX.GetHashCode();
                hash = 31 * hash + TEC.GetHashCode();
                hash = 31 * hash + MAG.GetHashCode();
                return 31 * hash + FTH.GetHashCode();
            }
        }

        /// <summary>
        /// Determines if the two given EquipStats have the same stat values.
        /// </summary>
        public static bool operator ==(EquipStats a, EquipStats b)
        {
            return a.VIT == b.VIT && a.STR == b.STR && a.DEX == b.DEX && a.TEC == b.TEC && a.MAG == b.MAG && a.FTH == b.FTH;
        }

        /// <summary>
        /// Determines if the two given EquipStats do not have the same stat values.
        /// </summary>
        public static bool operator !=(EquipStats a, EquipStats b)
        {
            return a.VIT != b.VIT || a.STR != b.STR || a.DEX != b.DEX || a.TEC != b.TEC || a.MAG != b.MAG || a.FTH != b.FTH;
        }

        /// <summary>
        /// Creates a new EquipStats by adding together the values of two other EquipStats.
        /// </summary>
        public static EquipStats operator +(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT + b.VIT, a.STR + b.STR, a.DEX + b.DEX, a.TEC + b.TEC, a.MAG + b.MAG, a.FTH + b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by subtracting the values of one EquipStats from another.
        /// </summary>
        public static EquipStats operator -(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT - b.VIT, a.STR - b.STR, a.DEX - b.DEX, a.TEC - b.TEC, a.MAG - b.MAG, a.FTH - b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by multiplying together the values of two other EquipStats.
        /// </summary>
        public static EquipStats operator *(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT * b.VIT, a.STR * b.STR, a.DEX * b.DEX, a.TEC * b.TEC, a.MAG * b.MAG, a.FTH * b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by diving the values of one EquipStats from another.
        /// </summary>
        public static EquipStats operator /(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT / b.VIT, a.STR / b.STR, a.DEX / b.DEX, a.TEC / b.TEC, a.MAG / b.MAG, a.FTH / b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by adding together the values of an EquipStats and an EquipStatsDouble. Truncates excess double values.
        /// </summary>
        public static EquipStats operator +(EquipStats a, EquipStatsDouble b)
        {
            return new EquipStats(a.VIT + (int)b.VIT, a.STR + (int)b.STR, a.DEX + (int)b.DEX, a.TEC + (int)b.TEC, a.MAG + (int)b.MAG, a.FTH + (int)b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by subtracting the values of an EquipStatsDouble and an EquipStats. Truncates excess double values.
        /// </summary>
        public static EquipStats operator -(EquipStats a, EquipStatsDouble b)
        {
            return new EquipStats(a.VIT - (int)b.VIT, a.STR - (int)b.STR, a.DEX - (int)b.DEX, a.TEC - (int)b.TEC, a.MAG - (int)b.MAG, a.FTH - (int)b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStats by multiplying together the values of an EquipStats and an EquipStatsDouble.
        /// </summary>
        public static EquipStats operator *(EquipStats a, EquipStatsDouble b)
        {
            return new EquipStats((int)Math.Round(a.VIT * b.VIT), (int)Math.Round(a.STR * b.STR), (int)Math.Round(a.DEX * b.DEX), (int)Math.Round(a.TEC * b.TEC), (int)Math.Round(a.MAG * b.MAG), (int)Math.Round(a.FTH * b.FTH));
        }

        /// <summary>
        /// Creates a new EquipStats by diving the values of an EquipStatsDouble from an EquipStats.
        /// </summary>
        public static EquipStats operator /(EquipStats a, EquipStatsDouble b)
        {
            return new EquipStats((int)Math.Round(a.VIT / b.VIT), (int)Math.Round(a.STR / b.STR), (int)Math.Round(a.DEX / b.DEX), (int)Math.Round(a.TEC / b.TEC), (int)Math.Round(a.MAG / b.MAG), (int)Math.Round(a.FTH / b.FTH));
        }

        /// <summary>
        /// Creates a new EquipStats by adding a value to all stats.
        /// </summary>
        public static EquipStats operator +(EquipStats a, int b)
        {
            return new EquipStats(a.VIT + b, a.STR + b, a.DEX + b, a.TEC + b, a.MAG + b, a.FTH + b);
        }

        /// <summary>
        /// Creates a new EquipStats by subtracting a value from all stats.
        /// </summary>
        public static EquipStats operator -(EquipStats a, int b)
        {
            return new EquipStats(a.VIT - b, a.STR - b, a.DEX - b, a.TEC - b, a.MAG - b, a.FTH - b);
        }

        /// <summary>
        /// Creates a new EquipStats by multiplying all stats by a value.
        /// </summary>
        public static EquipStats operator *(EquipStats a, double b)
        {
            return new EquipStats((int)Math.Round(a.VIT * b), (int)Math.Round(a.STR * b), (int)Math.Round(a.DEX * b), (int)Math.Round(a.TEC * b), (int)Math.Round(a.MAG * b), (int)Math.Round(a.FTH * b));
        }

        /// <summary>
        /// Creates a new EquipStats by dividing all stats by a value.
        /// </summary>
        public static EquipStats operator /(EquipStats a, double b)
        {
            return new EquipStats((int)Math.Round(a.VIT / b), (int)Math.Round(a.STR / b), (int)Math.Round(a.DEX / b), (int)Math.Round(a.TEC / b), (int)Math.Round(a.MAG / b), (int)Math.Round(a.FTH / b));
        }
    }

    /// <summary>
    /// Represents a set of all six of the player stats, as double-precision floating-point values.
    /// </summary>
    public struct EquipStatsDouble
    {
        /// <summary>
        /// EquipStatsDouble where all stats are 0.
        /// </summary>
        public static readonly EquipStatsDouble NONE = new EquipStatsDouble(0, 0, 0, 0, 0, 0);
        /// <summary>
        /// EquipStatsDouble where all stats are 1.
        /// </summary>
        public static readonly EquipStatsDouble ONE = new EquipStatsDouble(1, 1, 1, 1, 1, 1);

        private readonly double[] stats;
        /// <summary>
        /// The VIT stat.
        /// </summary>
        public double VIT { get { return stats != null ? stats[0] : 0; } }
        /// <summary>
        /// The STR stat.
        /// </summary>
        public double STR { get { return stats != null ? stats[1] : 0; } }
        /// <summary>
        /// The DEX stat.
        /// </summary>
        public double DEX { get { return stats != null ? stats[2] : 0; } }
        /// <summary>
        /// The TEC stat.
        /// </summary>
        public double TEC { get { return stats != null ? stats[3] : 0; } }
        /// <summary>
        /// The MAG stat.
        /// </summary>
        public double MAG { get { return stats != null ? stats[4] : 0; } }
        /// <summary>
        /// The FTH stat.
        /// </summary>
        public double FTH { get { return stats != null ? stats[5] : 0; } }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble that has the same stats as the given EquipStatsDouble.
        /// </summary>
        public EquipStatsDouble(EquipStatsDouble stats)
        {
            this.stats = stats.stats;
        }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble that has the same stats as the given EquipStats.
        /// </summary>
        public static implicit operator EquipStatsDouble(EquipStats stats)
        {
            return new EquipStatsDouble(stats.VIT, stats.STR, stats.DEX, stats.TEC, stats.MAG, stats.FTH);
        }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble that has the specified stats.
        /// </summary>
        public EquipStatsDouble(double VIT, double STR, double DEX, double TEC, double MAG, double FTH)
        {
            stats = new double[6];
            stats[0] = VIT;
            stats[1] = STR;
            stats[2] = DEX;
            stats[3] = TEC;
            stats[4] = MAG;
            stats[5] = FTH;
        }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble where all stats are the given value.
        /// </summary>
        public EquipStatsDouble(double stat)
        {
            stats = new double[6];
            stats[0] = stat;
            stats[1] = stat;
            stats[2] = stat;
            stats[3] = stat;
            stats[4] = stat;
            stats[5] = stat;
        }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble from an array of stat values. The array must be of length 6.
        /// </summary>
        public EquipStatsDouble(int[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            stats = statArray.Select(x => (double)x).ToArray();
        }

        /// <summary>
        /// Creates a new instance of EquipStatsDouble from an array of stat values. The array must be of length 6.
        /// </summary>
        public EquipStatsDouble(double[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            stats = (double[])statArray.Clone();
        }

        /// <summary>
        /// Returns the stats as an array of values. Will be of length 6.
        /// </summary>
        public double[] GetStatArray()
        {
            return stats != null ? (double[])stats.Clone() : new double[6];
        }

        /// <summary>
        /// Gets the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public double GetByIndex(int index)
        {
            return stats[index];
        }

        /// <summary>
        /// Sets the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public void SetByIndex(int index, double stat)
        {
            stats[index] = stat;
        }

        /// <summary>
        /// Adds to the stat with the specified index. 0 is VIT, 1 is STR, 2 is DEX, 3 is TEC, 4 is MAG, 5 is FTH.
        /// </summary>
        public void AddByIndex(int index, double stat)
        {
            stats[index] += stat;
        }

        /// <summary>
        /// Adds this EquipStatsDouble's values to the given stat value array. The array must be of length 6.
        /// </summary>
        public double[] AddTo(double[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            statArray[0] += VIT;
            statArray[1] += STR;
            statArray[2] += DEX;
            statArray[3] += TEC;
            statArray[4] += MAG;
            statArray[5] += FTH;
            return statArray;
        }

        /// <summary>
        /// Subtracts this EquipStatsDouble's values from the given stat value array. The array must be of length 6.
        /// </summary>
        public double[] SubtractFrom(double[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            statArray[0] -= VIT;
            statArray[1] -= STR;
            statArray[2] -= DEX;
            statArray[3] -= TEC;
            statArray[4] -= MAG;
            statArray[5] -= FTH;
            return statArray;
        }

        /// <summary>
        /// Determines if this EquipStatsDouble and the given EquipStatsDouble have the same stat values.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is EquipStatsDouble stats ? this == stats : base.Equals(obj);
        }

        /// <summary>
        /// Gets a hash code for this EquipStatsDouble.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = VIT.GetHashCode();
                hash = 31 * hash + STR.GetHashCode();
                hash = 31 * hash + DEX.GetHashCode();
                hash = 31 * hash + TEC.GetHashCode();
                hash = 31 * hash + MAG.GetHashCode();
                return 31 * hash + FTH.GetHashCode();
            }
        }

        /// <summary>
        /// Determines if the two given EquipStatsDouble have the same stat values.
        /// </summary>
        public static bool operator ==(EquipStatsDouble a, EquipStatsDouble b)
        {
            return a.VIT == b.VIT && a.STR == b.STR && a.DEX == b.DEX && a.TEC == b.TEC && a.MAG == b.MAG && a.FTH == b.FTH;
        }

        /// <summary>
        /// Determines if the two given EquipStatsDouble do not have the same stat values.
        /// </summary>
        public static bool operator !=(EquipStatsDouble a, EquipStatsDouble b)
        {
            return a.VIT != b.VIT || a.STR != b.STR || a.DEX != b.DEX || a.TEC != b.TEC || a.MAG != b.MAG || a.FTH != b.FTH;
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by adding together the values of two other EquipStatsDouble.
        /// </summary>
        public static EquipStatsDouble operator +(EquipStatsDouble a, EquipStatsDouble b)
        {
            return new EquipStatsDouble(a.VIT + b.VIT, a.STR + b.STR, a.DEX + b.DEX, a.TEC + b.TEC, a.MAG + b.MAG, a.FTH + b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by subtracting the values of one EquipStatsDouble from another.
        /// </summary>
        public static EquipStatsDouble operator -(EquipStatsDouble a, EquipStatsDouble b)
        {
            return new EquipStatsDouble(a.VIT - b.VIT, a.STR - b.STR, a.DEX - b.DEX, a.TEC - b.TEC, a.MAG - b.MAG, a.FTH - b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by multiplying together the values of two other EquipStatsDouble.
        /// </summary>
        public static EquipStatsDouble operator *(EquipStatsDouble a, EquipStatsDouble b)
        {
            return new EquipStatsDouble(a.VIT * b.VIT, a.STR * b.STR, a.DEX * b.DEX, a.TEC * b.TEC, a.MAG * b.MAG, a.FTH * b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by diving the values of one EquipStatsDouble from another.
        /// </summary>
        public static EquipStatsDouble operator /(EquipStatsDouble a, EquipStatsDouble b)
        {
            return new EquipStatsDouble(a.VIT / b.VIT, a.STR / b.STR, a.DEX / b.DEX, a.TEC / b.TEC, a.MAG / b.MAG, a.FTH / b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by adding together the values of an EquipStatsDouble and an EquipStats.
        /// </summary>
        public static EquipStatsDouble operator +(EquipStatsDouble a, EquipStats b)
        {
            return new EquipStatsDouble(a.VIT + b.VIT, a.STR + b.STR, a.DEX + b.DEX, a.TEC + b.TEC, a.MAG + b.MAG, a.FTH + b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by subtracting the values of an EquipStats from an EquipStatsDouble.
        /// </summary>
        public static EquipStatsDouble operator -(EquipStatsDouble a, EquipStats b)
        {
            return new EquipStatsDouble(a.VIT - b.VIT, a.STR - b.STR, a.DEX - b.DEX, a.TEC - b.TEC, a.MAG - b.MAG, a.FTH - b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by multiplying together an EquipStatsDouble and an EquipStats.
        /// </summary>
        public static EquipStatsDouble operator *(EquipStatsDouble a, EquipStats b)
        {
            return new EquipStatsDouble(a.VIT * b.VIT, a.STR * b.STR, a.DEX * b.DEX, a.TEC * b.TEC, a.MAG * b.MAG, a.FTH * b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by diving the values of an EquipStats from an EquipStatsDouble.
        /// </summary>
        public static EquipStatsDouble operator /(EquipStatsDouble a, EquipStats b)
        {
            return new EquipStatsDouble(a.VIT / b.VIT, a.STR / b.STR, a.DEX / b.DEX, a.TEC / b.TEC, a.MAG / b.MAG, a.FTH / b.FTH);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by adding a value to all stats.
        /// </summary>
        public static EquipStatsDouble operator +(EquipStatsDouble a, double b)
        {
            return new EquipStatsDouble(a.VIT + b, a.STR + b, a.DEX + b, a.TEC + b, a.MAG + b, a.FTH + b);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by subtracting a value from all stats.
        /// </summary>
        public static EquipStatsDouble operator -(EquipStatsDouble a, double b)
        {
            return new EquipStatsDouble(a.VIT - b, a.STR - b, a.DEX - b, a.TEC - b, a.MAG - b, a.FTH - b);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by multiplying all stats by a value.
        /// </summary>
        public static EquipStatsDouble operator *(EquipStatsDouble a, double b)
        {
            return new EquipStatsDouble(a.VIT * b, a.STR * b, a.DEX * b, a.TEC * b, a.MAG * b, a.FTH * b);
        }

        /// <summary>
        /// Creates a new EquipStatsDouble by dividing all stats by a value.
        /// </summary>
        public static EquipStatsDouble operator /(EquipStatsDouble a, double b)
        {
            return new EquipStatsDouble(a.VIT / b, a.STR / b, a.DEX / b, a.TEC / b, a.MAG / b, a.FTH / b);
        }
    }
}
