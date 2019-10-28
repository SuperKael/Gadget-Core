using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SwapItem")]
    static class Patch_GameScript_SwapItem
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
            if (ItemRegistry.GetSingleton().HasEntry(___holdingItem.id) || ItemRegistry.GetSingleton().HasEntry(___inventory[slot].id))
            {
                ItemInfo itemInfo = ItemRegistry.GetSingleton().GetEntry(___holdingItem.id);
                ItemInfo slotInfo = ItemRegistry.GetSingleton().GetEntry(___inventory[slot].id);
                ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
                ItemType slotItemType = slotInfo != null ? (slotInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___inventory[slot].id);
                if ((slot == 36 && holdingItemType != ItemType.WEAPON) || (slot == 37 && (holdingItemType != ItemType.OFFHAND)) || (slot == 38 && (holdingItemType != ItemType.HELMET)) || (slot == 39 && (holdingItemType != ItemType.ARMOR)) || ((slot == 40 || slot == 41) && (holdingItemType != ItemType.RING)) || (slot > 41 && (holdingItemType != ItemType.DROID)))
                {
                    __instance.Error(2);
                }
                else
                {
                    Item item = ___holdingItem;
                    ___holdingItem = ___inventory[slot];
                    ___inventory[slot] = item;
                    ItemType typeHolder = holdingItemType;
                    holdingItemType = slotItemType;
                    slotItemType = typeHolder;
                    RefreshSlot.Invoke(__instance, new object[] { slot });
                    RefreshHoldingSlot.Invoke(__instance, new object[] { });
                    if (slot > 41 && slot < 45 && slotItemType == ItemType.DROID)
                    {
                        if (holdingItemType == ItemType.DROID)
                        {
                            MonoBehaviour.print("swapped droids");
                            UpdateDroids.Invoke(__instance, new object[] { });
                            __instance.droid[slot - 42].SendMessage("SetStats", ___inventory[slot].id);
                        }
                        else
                        {
                            ___droidCount++;
                            UpdateDroids.Invoke(__instance, new object[] { });
                            __instance.droid[slot - 42].SendMessage("SetStats", ___inventory[slot].id);
                        }
                    }
                    if (slot > 35)
                    {
                        if (slot > 41)
                        {
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/placeD"), Menuu.soundLevel / 10f);
                        }
                        else
                        {
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/bling"), Menuu.soundLevel / 10f);
                        }
                        UnityEngine.Object.Instantiate(Resources.Load("clickBurst"), new Vector3(__instance.invIcon[slot].transform.position.x, __instance.invIcon[slot].transform.position.y, 0f), Quaternion.identity);
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
                        int[] gearBaseStats = (int[])GetGearBaseStats.Invoke(__instance, new object[] { ___inventory[slot].id });
                        int[] gearBaseStats2 = (int[])GetGearBaseStats.Invoke(__instance, new object[] { ___holdingItem.id });
                        int num = (int)GetItemLevel.Invoke(__instance, new object[] { ___inventory[slot].exp });
                        int num2 = (int)GetItemLevel.Invoke(__instance, new object[] { ___holdingItem.exp });
                        if (slot > 41)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (gearBaseStats[i] > 0)
                                {
                                    GameScript.GEARSTAT[i] += gearBaseStats[i];
                                    __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                                }
                                if (gearBaseStats2[i] > 0)
                                {
                                    GameScript.GEARSTAT[i] -= gearBaseStats2[i];
                                    __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (gearBaseStats[i] > 0)
                                {
                                    GameScript.GEARSTAT[i] += ___inventory[slot].tier * 3 + gearBaseStats[i] * num;
                                    __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                                }
                                if (gearBaseStats2[i] > 0)
                                {
                                    GameScript.GEARSTAT[i] -= ___holdingItem.tier * 3 + gearBaseStats2[i] * num2;
                                    __instance.txtPlayerStat[i].GetComponent<Animation>().Play();
                                }
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                ItemInfo slotAspect = ItemRegistry.GetSingleton().GetEntry(___inventory[slot].aspect[j]);
                                ItemInfo heldAspect = ItemRegistry.GetSingleton().GetEntry(___holdingItem.aspect[j]);
                                for (int i = 0; i < 6; i++)
                                {
                                    if (slotAspect != null)
                                    {
                                        GameScript.GEARSTAT[i] += slotAspect.Stats.GetByIndex(i);
                                    }
                                    else if (___inventory[slot].aspect[j] - 200 == i + 1)
                                    {
                                        GameScript.GEARSTAT[i] += ___inventory[slot].aspectLvl[j];
                                    }
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
                        RefreshMODS.Invoke(__instance, new object[] { });
                        __instance.UpdateHP();
                        __instance.UpdateEnergy();
                        __instance.UpdateMana();
                        Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                        MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                        {
                GameScript.equippedIDs,
                0,
                GameScript.dead
                        });
                        if (slotInfo != null) slotInfo.InvokeOnDequip(slot);
                        if (itemInfo != null) itemInfo.InvokeOnEquip(slot);
                    }
                    else
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK1"), Menuu.soundLevel / 10f);
                    }
                }
                return false;
            }
            return true;
        }
    }
}