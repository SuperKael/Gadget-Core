using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SelectItem")]
    internal static class Patch_GameScript_SelectItem
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo RefreshHoldingSlot = typeof(GameScript).GetMethod("RefreshHoldingSlot", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo UpdateDroids = typeof(GameScript).GetMethod("UpdateDroids", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo GetGearBaseStats = typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo GetItemLevel = typeof(GameScript).GetMethod("GetItemLevel", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo RefreshStats = typeof(GameScript).GetMethod("RefreshStats", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo RefreshMODS = typeof(GameScript).GetMethod("RefreshMODS", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___inventory, ref int ___droidCount)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___inventory[slot].id);
            ItemType slotItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___inventory[slot].id);
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK1"), Menuu.soundLevel / 10f);
            ___holdingItem = ___inventory[slot];
            ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
            RefreshSlot.Invoke(__instance, new object[] { slot });
            RefreshHoldingSlot.Invoke(__instance, new object[] { });
            if (slotItemType == ItemType.DROID && slot > 41 && slot < 45)
            {
                ___droidCount--;
                __instance.activated[slot - 42] = false;
                __instance.droid[slot - 42].SendMessage("Deactivate", SendMessageOptions.DontRequireReceiver);
                UpdateDroids.Invoke(__instance, new object[] { });
            }
            if (slot > 35)
            {
                if (slot == 36)
                {
                    GameScript.equippedIDs[0] = ___inventory[slot].id;
                }
                else if (slot == 37)
                {
                    GameScript.equippedIDs[1] = ___inventory[slot].id;
                }
                else if (slot == 38)
                {
                    GameScript.equippedIDs[2] = ___inventory[slot].id;
                }
                else if (slot == 39)
                {
                    GameScript.equippedIDs[3] = ___inventory[slot].id;
                }
                int[] stats = GadgetCoreAPI.equippedGearStats[slot - 36];
                for (int i = 0; i < 6; i++)
                {
                    if (stats[i] > 0)
                    {
                        GameScript.GEARSTAT[i] -= stats[i];
                        __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                    }
                }
                GadgetCoreAPI.equippedGearStats[slot - 36] = new[] { 0, 0, 0, 0, 0, 0 };
                RefreshStats.Invoke(__instance, new object[] { });
                Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                int[] convertedIDs = new[]
                {
                    ItemRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[0]),
                    ItemRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[1]),
                    ItemRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[2]),
                    ItemRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[3]),
                    CharacterRaceRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[4]),
                    GameScript.equippedIDs[5],
                    CharacterUniformRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[6]),
                    CharacterAugmentRegistry.Singleton.ConvertIDToHost(GameScript.equippedIDs[7])
                };
                MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                {
                        convertedIDs,
                        0,
                        GameScript.dead
                });
                RefreshMODS.Invoke(__instance, new object[] { });
                __instance.UpdateHP();
                __instance.UpdateEnergy();
                __instance.UpdateMana();
                if (itemInfo != null) itemInfo.InvokeOnDequip(slot);
            }
            return false;
        }
    }
}