using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public static class LootTables
    {
        private static Dictionary<string, List<LootTableEntry>> lootTables = new Dictionary<string, List<LootTableEntry>>();

        public static void DropLoot(string tableID, Vector3 pos, float dropFrequency = 0.0f)
        {
            if (lootTables.ContainsKey(tableID))
            {
                if (dropFrequency <= 0.0f)
                {
                    if (lootTables.ContainsKey(tableID)) foreach (LootTableEntry tableEntry in lootTables[tableID])
                    {
                        tableEntry.TryDrop(pos);
                    }
                    if (lootTables.ContainsKey(tableID.Split(':')[0] + ":all")) foreach (LootTableEntry tableEntry in lootTables[tableID.Split(':')[0] + ":all"])
                    {
                        tableEntry.TryDrop(pos);
                    }
                    if (lootTables.ContainsKey("all")) foreach (LootTableEntry tableEntry in lootTables["all"])
                    {
                        tableEntry.TryDrop(pos);
                    }
                }
                else
                {
                    InstanceTracker.playerScript.StartCoroutine(DelayDropLoot(tableID, pos, dropFrequency));
                }
            }
        }

        private static IEnumerator DelayDropLoot(string tableID, Vector3 pos, float dropFrequency)
        {
            if (lootTables.ContainsKey(tableID)) foreach (LootTableEntry tableEntry in lootTables[tableID])
            {
                if (tableEntry.TryDrop(pos)) yield return new WaitForSeconds(dropFrequency);
            }
            if (lootTables.ContainsKey(tableID.Split(':')[0] + ":all")) foreach (LootTableEntry tableEntry in lootTables[tableID.Split(':')[0] + ":all"])
            {
                if (tableEntry.TryDrop(pos)) yield return new WaitForSeconds(dropFrequency);
            }
            if (lootTables.ContainsKey("all")) foreach (LootTableEntry tableEntry in lootTables["all"])
            {
                if (tableEntry.TryDrop(pos)) yield return new WaitForSeconds(dropFrequency);
            }
            yield break;
        }

        public static ItemInfo AddToLootTable(this ItemInfo item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(new Item(item.ID, 1, 0, 0, 0, new int[3], new int[3]), dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, false));
            return item;
        }

        public static void AddItemToLootTable(Item item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(item, dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, false));
        }

        public static ChipInfo AddToLootTable(this ChipInfo item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(new Item(item.ID, 1, 0, 0, 0, new int[3], new int[3]), dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, true));
            return item;
        }

        public static void AddChipToLootTable(Item item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(item, dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, true));
        }

        private struct LootTableEntry
        {
            public readonly Item itemToDrop;
            public readonly float dropChance;
            public readonly int minDropQuantity, maxDropQuantity;
            public readonly Func<Vector3, bool> CheckValidToDrop;
            public readonly Func<Item, Vector3, bool> CustomDropBehavior;
            public readonly bool isChip;

            public LootTableEntry(Item itemToDrop, float dropChance, int minDropQuantity, int maxDropQuantity, Func<Vector3, bool> CheckValidToDrop, Func<Item, Vector3, bool> CustomDropBehavior, bool isChip = false)
            {
                this.itemToDrop = itemToDrop;
                this.dropChance = dropChance;
                this.minDropQuantity = minDropQuantity;
                this.maxDropQuantity = maxDropQuantity;
                this.CheckValidToDrop = CheckValidToDrop;
                this.CustomDropBehavior = CustomDropBehavior;
                this.isChip = isChip;
            }

            public bool TryDrop(Vector3 pos)
            {
                if (CheckValidToDrop == null || CheckValidToDrop(pos))
                {
                    if (dropChance <= 0) return false;
                    if (UnityEngine.Random.value <= dropChance)
                    {
                        itemToDrop.q = Mathf.RoundToInt(UnityEngine.Random.value * (maxDropQuantity - minDropQuantity)) + minDropQuantity;
                        if (CustomDropBehavior == null || CustomDropBehavior(itemToDrop, pos))
                        {
                            GadgetCoreAPI.SpawnItemLocal(pos, itemToDrop, isChip);
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
