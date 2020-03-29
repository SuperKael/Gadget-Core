using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SelectItem")]
    static class Patch_GameScript_SelectItem
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshHoldingSlot = typeof(GameScript).GetMethod("RefreshHoldingSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo UpdateDroids = typeof(GameScript).GetMethod("UpdateDroids", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetGearBaseStats = typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo GetItemLevel = typeof(GameScript).GetMethod("GetItemLevel", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshStats = typeof(GameScript).GetMethod("RefreshStats", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo RefreshMODS = typeof(GameScript).GetMethod("RefreshMODS", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___inventory, ref int ___droidCount)
        {
            if (ItemRegistry.GetSingleton().HasEntry(___inventory[slot].id))
            {
                ItemInfo itemInfo = ItemRegistry.GetSingleton().GetEntry(___inventory[slot].id);
                ItemType slotItemType = itemInfo != null ? (itemInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___inventory[slot].id);
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
                    int[] gearBaseStats = (int[])GetGearBaseStats.Invoke(__instance, new object[] { ___holdingItem.id });
                    if (slot > 41)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if (gearBaseStats[i] > 0)
                            {
                                GameScript.GEARSTAT[i] -= gearBaseStats[i];
                                __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                            }
                        }
                    }
                    else
                    {
                        int num = (int)GetItemLevel.Invoke(__instance, new object[] { ___holdingItem.exp });
                        for (int i = 0; i < 6; i++)
                        {
                            if (gearBaseStats[i] > 0)
                            {
                                GameScript.GEARSTAT[i] -= ___holdingItem.tier * 3 + gearBaseStats[i] * num;
                                __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                            }
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            ItemInfo heldAspect = ItemRegistry.GetSingleton().GetEntry(___holdingItem.aspect[j]);
                            for (int i = 0; i < 6; i++)
                            {
                                if (heldAspect != null)
                                {
                                    GameScript.GEARSTAT[i] -= heldAspect.Stats.GetByIndex(i);
                                }
                                if (___holdingItem.aspect[j] - 200 == i + 1)
                                {
                                    GameScript.GEARSTAT[i] -= ___holdingItem.aspectLvl[j];
                                }
                            }
                        }
                    }
                    RefreshStats.Invoke(__instance, new object[] { });
                    Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                    MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                    {
            GameScript.equippedIDs,
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
            return true;
        }
    }
}