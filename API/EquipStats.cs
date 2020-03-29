using System;

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
            stats = statArray;
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
        public int GetByIndex(int index)
        {
            return stats[index];
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
        public static EquipStats operator *(EquipStats a, float b)
        {
            return new EquipStats((int)(a.VIT * b), (int)(a.STR * b), (int)(a.DEX * b), (int)(a.TEC * b), (int)(a.MAG * b), (int)(a.FTH * b));
        }

        /// <summary>
        /// Creates a new EquipStats by dividing all stats by a value.
        /// </summary>
        public static EquipStats operator /(EquipStats a, float b)
        {
            return new EquipStats((int)(a.VIT / b), (int)(a.STR / b), (int)(a.DEX / b), (int)(a.TEC / b), (int)(a.MAG / b), (int)(a.FTH / b));
        }
    }
}
