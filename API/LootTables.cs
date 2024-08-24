using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Used for management of loot tables.
    /// </summary>
    public static class LootTables
    {
        private static Dictionary<string, List<LootTableEntry>> lootTables = new Dictionary<string, List<LootTableEntry>>();

        /// <summary>
        /// Call to cause a loot table's contents to drop at the given location. dropFrequency specifies how much to delay between each dropped item.
        /// </summary>
        public static void DropLoot(string tableID, Vector3 pos, float dropFrequency = 0.0f)
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
                InstanceTracker.PlayerScript.StartCoroutine(DelayDropLoot(tableID, pos, dropFrequency));
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
        }

        /// <summary>
        /// Adds the item to a drop table. Note that maxDropQuantity is optional, and if left unspecified minDropQuantity will always be dropped. Also returns the ItemInfo this was called on, for convenience in chaining calls.
        /// </summary>
        /// <param name="item">The item to add to the drop table. Note that the quantity specified is entirely ignored. This may be null, if you wish to entirely rely upon CustomDropBehavior</param>
        /// <param name="tableID">The loot table to add this item to.</param>
        /// <param name="dropChance">The chance for this item to drop from 0.0 to 1.0. If dropChance is higher than 1.0, then it will still just always drop. Similarly, if it is less than 0.0, then it will simply never drop.</param>
        /// <param name="minDropQuantity">The minimum quantity to be dropped. If maxDropQuantity is not specified, then this will always be the quantity that is dropped.</param>
        /// <param name="maxDropQuantity">The maximum quantity to be dropped. If this is set, then a random quantity from minDropQuantity to maxDropQuantity will be dropped.</param>
        /// <param name="CheckValidToDrop">Use to add custom behavior to check whether to drop an item in a given circumstance. This is always called when the loot table is dropped, even if the drop chance fails. Receives the position to be dropped at as a parameter.</param>
        /// <param name="CustomDropBehavior">Use to add custom behavior for how to drop the item(s). This is only called after it has been confirmed that an item will drop based on the drop chance and CheckValidToDrop. This will be called once for each item dropped in the case of non-stackable items. Receives the position to be dropped at, as well as the Item with the quantity set, as parameters.</param>
        public static ItemInfo AddToLootTable(this ItemInfo item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(new Item(item.ID, 1, 0, 0, 0, new int[3], new int[3]), dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, false));
            return item;
        }

        /// <summary>
        /// Adds the item to a drop table. Note that maxDropQuantity is optional, and if left unspecified minDropQuantity will always be dropped.
        /// </summary>
        /// <param name="item">The item to add to the drop table. Note that the quantity specified is entirely ignored. This may be null, if you wish to entirely rely upon CustomDropBehavior</param>
        /// <param name="tableID">The loot table to add this item to.</param>
        /// <param name="dropChance">The chance for this item to drop from 0.0 to 1.0. If dropChance is higher than 1.0, then it will still just always drop. Similarly, if it is less than 0.0, then it will simply never drop.</param>
        /// <param name="minDropQuantity">The minimum quantity to be dropped. If maxDropQuantity is not specified, then this will always be the quantity that is dropped.</param>
        /// <param name="maxDropQuantity">The maximum quantity to be dropped. If this is set, then a random quantity from minDropQuantity to maxDropQuantity will be dropped.</param>
        /// <param name="CheckValidToDrop">Use to add custom behavior to check whether to drop an item in a given circumstance. This is always called when the loot table is dropped, even if the drop chance fails. Receives the position to be dropped at as a parameter.</param>
        /// <param name="CustomDropBehavior">Use to add custom behavior for how to drop the item(s). This is only called after it has been confirmed that an item will drop based on the drop chance and CheckValidToDrop. This will be called once for each item dropped in the case of non-stackable items.  Receives the position to be dropped at, as well as the Item with the quantity set, as parameters.</param>
        public static void AddItemToLootTable(Item item, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(item, dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, false));
        }

        /// <summary>
        /// Adds the chip to a drop table. Note that maxDropQuantity is optional, and if left unspecified minDropQuantity will always be dropped. Also returns the ChipInfo this was called on, for convenience in chaining calls.
        /// </summary>
        /// <param name="chip">The chip to add to the drop table.</param>
        /// <param name="tableID">The loot table to add this item to.</param>
        /// <param name="dropChance">The chance for this item to drop from 0.0 to 1.0. If dropChance is higher than 1.0, then it will still just always drop. Similarly, if it is less than 0.0, then it will simply never drop.</param>
        /// <param name="minDropQuantity">The minimum quantity to be dropped. If maxDropQuantity is not specified, then this will always be the quantity that is dropped.</param>
        /// <param name="maxDropQuantity">The maximum quantity to be dropped. If this is set, then a random quantity from minDropQuantity to maxDropQuantity will be dropped.</param>
        /// <param name="CheckValidToDrop">Use to add custom behavior to check whether to drop a chip in a given circumstance. This is always called when the loot table is dropped, even if the drop chance fails. Receives the position to be dropped at as a parameter.</param>
        /// <param name="CustomDropBehavior">Use to add custom behavior for how to drop the chip(s). This is only called after it has been confirmed that an item will drop based on the drop chance and CheckValidToDrop. This will be called once for each chip dropped. Receives the position to be dropped at, as well as the Item with the quantity set, as parameters.</param>
        public static ChipInfo AddToLootTable(this ChipInfo chip, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(new Item(chip.ID, 1, 0, 0, 0, new int[3], new int[3]), dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, true));
            return chip;
        }

        /// <summary>
        /// Adds the chip to a drop table. Note that maxDropQuantity is optional, and if left unspecified minDropQuantity will always be dropped.
        /// </summary>
        /// <param name="chip">The chip to add to the drop table.</param>
        /// <param name="tableID">The loot table to add this item to.</param>
        /// <param name="dropChance">The chance for this item to drop from 0.0 to 1.0. If dropChance is higher than 1.0, then it will still just always drop. Similarly, if it is less than 0.0, then it will simply never drop.</param>
        /// <param name="minDropQuantity">The minimum quantity to be dropped. If maxDropQuantity is not specified, then this will always be the quantity that is dropped.</param>
        /// <param name="maxDropQuantity">The maximum quantity to be dropped. If this is set, then a random quantity from minDropQuantity to maxDropQuantity will be dropped.</param>
        /// <param name="CheckValidToDrop">Use to add custom behavior to check whether to drop a chip in a given circumstance. This is always called when the loot table is dropped, even if the drop chance fails. Receives the position to be dropped at as a parameter.</param>
        /// <param name="CustomDropBehavior">Use to add custom behavior for how to drop the chip(s). This is only called after it has been confirmed that an item will drop based on the drop chance and CheckValidToDrop. This will be called once for each chip dropped. Receives the position to be dropped at, as well as the Item with the quantity set, as parameters.</param>
        public static void AddChipToLootTable(int chip, string tableID, float dropChance, int minDropQuantity, int maxDropQuantity = -1, Func<Vector3, bool> CheckValidToDrop = null, Func<Item, Vector3, bool> CustomDropBehavior = null)
        {
            if (!lootTables.ContainsKey(tableID)) lootTables.Add(tableID, new List<LootTableEntry>());
            lootTables[tableID].Add(new LootTableEntry(new Item(chip, 1, 0, 0, 0, new int[3], new int[3]), dropChance, minDropQuantity, maxDropQuantity > minDropQuantity ? maxDropQuantity : minDropQuantity, CheckValidToDrop, CustomDropBehavior, true));
        }

        internal static void RemoveModEntries(int modID)
        {
            foreach (string tableID in lootTables.Keys.ToArray())
            {
                foreach (LootTableEntry entry in lootTables[tableID].Where(x => x.modID == modID).ToArray())
                {
                    lootTables[tableID].Remove(entry);
                    if (lootTables[tableID].Count < 1) lootTables.Remove(tableID);
                }
            }
        }

        private struct LootTableEntry
        {
            public readonly Item itemToDrop;
            public readonly float dropChance;
            public readonly int minDropQuantity, maxDropQuantity;
            public readonly Func<Vector3, bool> CheckValidToDrop;
            public readonly Func<Item, Vector3, bool> CustomDropBehavior;
            public readonly bool isChip;
            public readonly int modID;

            public LootTableEntry(Item itemToDrop, float dropChance, int minDropQuantity, int maxDropQuantity, Func<Vector3, bool> CheckValidToDrop, Func<Item, Vector3, bool> CustomDropBehavior, bool isChip = false)
            {
                if (!Registry.registeringVanilla && Registry.gadgetRegistering < 0) throw new InvalidOperationException("Loot table entries can only be added during Gadget initialization!");
                this.itemToDrop = itemToDrop;
                this.dropChance = dropChance;
                this.minDropQuantity = minDropQuantity;
                this.maxDropQuantity = maxDropQuantity;
                this.CheckValidToDrop = CheckValidToDrop;
                this.CustomDropBehavior = CustomDropBehavior;
                this.isChip = isChip;
                modID = Registry.gadgetRegistering;
            }

            public bool TryDrop(Vector3 pos)
            {
                if (CheckValidToDrop == null || CheckValidToDrop(pos))
                {
                    if (dropChance <= 0) return false;
                    if (UnityEngine.Random.value <= dropChance)
                    {
                        if (itemToDrop == null || isChip || (ItemRegistry.GetTypeByID(itemToDrop.id) & ItemType.NONSTACKING) == ItemType.NONSTACKING)
                        {
                            if (itemToDrop != null) itemToDrop.q = 1;
                            for (int i = 0; i < Mathf.RoundToInt(UnityEngine.Random.value * (maxDropQuantity - minDropQuantity)) + minDropQuantity; i++)
                            {
                                if ((CustomDropBehavior == null || CustomDropBehavior(itemToDrop, pos)) && itemToDrop != null)
                                {
                                    GadgetCoreAPI.SpawnItemLocal(pos, itemToDrop, isChip);
                                }
                            }
                        }
                        else
                        {
                            itemToDrop.q = Mathf.RoundToInt(UnityEngine.Random.value * (maxDropQuantity - minDropQuantity)) + minDropQuantity;
                            if ((CustomDropBehavior == null || CustomDropBehavior(itemToDrop, pos)) && itemToDrop != null)
                            {
                                GadgetCoreAPI.SpawnItemLocal(pos, itemToDrop, isChip);
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
