using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GadgetCore.API
{
    public struct EquipStats
    {
        public static readonly EquipStats NONE = new EquipStats(0, 0, 0, 0, 0, 0);

        private readonly int[] stats;
        public int VIT { get { return stats != null ? stats[0] : 0; } }
        public int STR { get { return stats != null ? stats[1] : 0; } }
        public int DEX { get { return stats != null ? stats[2] : 0; } }
        public int TEC { get { return stats != null ? stats[3] : 0; } }
        public int MAG { get { return stats != null ? stats[4] : 0; } }
        public int FTH { get { return stats != null ? stats[5] : 0; } }

        public EquipStats(EquipStats stats)
        {
            this.stats = stats.stats;
        }

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

        public EquipStats(int[] statArray)
        {
            if (statArray == null || statArray.Length != 6) throw new InvalidOperationException("Stat array must be of length 6!");
            stats = statArray;
        }

        public int[] GetStatArray()
        {
            return stats != null ? (int[])stats.Clone() : new int[6];
        }

        public int GetByIndex(int index)
        {
            return stats[index];
        }

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

        public static EquipStats operator +(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT + b.VIT, a.STR + b.STR, a.DEX + b.DEX, a.TEC + b.TEC, a.MAG + b.MAG, a.FTH + b.FTH);
        }

        public static EquipStats operator -(EquipStats a, EquipStats b)
        {
            return new EquipStats(a.VIT - b.VIT, a.STR - b.STR, a.DEX - b.DEX, a.TEC - b.TEC, a.MAG - b.MAG, a.FTH - b.FTH);
        }
    }
}
