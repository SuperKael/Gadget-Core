using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ShiftClick")]
    static class Patch_GameScript_ShiftClick
    {
        private static Item itemInSlot;

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref bool ___shiftclicking, Item[] ___inventory, ref Item[] ___storage, int slot, int ___curStoragePage, ref int ___droidCount, ref IEnumerator __result)
        {
            itemInSlot = ___inventory[slot];
            if (!___shiftclicking && ItemRegistry.GetSingleton().HasEntry(itemInSlot.id))
            {
                ItemInfo slotInfo = ItemRegistry.GetSingleton().GetEntry(itemInSlot.id);
                ItemType slotItemType = slotInfo != null ? (slotInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(itemInSlot.id);
                int num = 0;
                int num2 = ___curStoragePage * 30;
                int num3 = num2 + 30;
                if ((slotItemType & ItemType.NONSTACKING) == ItemType.STACKING)
                {
                    bool flag1 = false, flag2 = false;
                    for (int i = num2; i < num3; i++)
                    {
                        if (___storage[i].id == itemInSlot.id && ___storage[i].q < 9999)
                        {
                            if (___storage[i].q + ___inventory[slot].q <= 9999)
                            {
                                flag1 = true;
                                num = i;
                            }
                            else
                            {
                                if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___inventory[slot].q -= 9999 - ___storage[i].q;
                                ___storage[i].q = 9999;
                                flag2 = true;
                            }
                            break;
                        }
                    }
                    if (flag1)
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                        ___storage[num].q += ___inventory[slot].q;
                        ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                    }
                    else
                    {
                        for (int i = num2; i < num3; i++)
                        {
                            if (___storage[i].id == 0)
                            {
                                if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___storage[i] = ___inventory[slot];
                                ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                break;
                            }
                            else if (___storage[i].id == itemInSlot.id && ___storage[i].q < 9999)
                            {
                                if (___storage[i].q + ___inventory[slot].q <= 9999)
                                {
                                    if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                    ___storage[i].q += ___inventory[slot].q;
                                    ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                    break;
                                }
                                else
                                {
                                    if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                    ___inventory[slot].q -= 9999 - ___storage[i].q;
                                    ___storage[i].q = 9999;
                                }
                                flag2 = true;
                            }
                        }
                    }
                    typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                    __instance.StartCoroutine((IEnumerator)typeof(GameScript).GetMethod("RefreshStoragePage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ___curStoragePage }));
                }
                else
                {
                    for (int i = num2; i < num3; i++)
                    {
                        if (___storage[i].id == 0)
                        {
                            if (slot > 35)
                            {
                                if (slot == 36)
                                {
                                    GameScript.equippedIDs[0] = 0;
                                }
                                else if (slot == 37)
                                {
                                    GameScript.equippedIDs[1] = 0;
                                }
                                else if (slot == 38)
                                {
                                    GameScript.equippedIDs[2] = 0;
                                }
                                else if (slot == 39)
                                {
                                    GameScript.equippedIDs[3] = 0;
                                }
                                else if (slot == 40)
                                {
                                    GameScript.equippedIDs[4] = 0;
                                }
                                else if (slot == 41)
                                {
                                    GameScript.equippedIDs[5] = 0;
                                }
                                int[] gearBaseStats = (int[]) typeof(GameScript).GetMethod("GetGearBaseStats", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ___inventory[slot].id });
                                int num4 = (int)typeof(GameScript).GetMethod("GetItemLevel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ___inventory[slot].exp });
                                for (int j = 0; j < 6; j++)
                                {
                                    if (gearBaseStats[j] > 0)
                                    {
                                        GameScript.GEARSTAT[j] -= ___inventory[slot].tier * 3 + gearBaseStats[j] * num4;
                                        __instance.txtPlayerStat[j].GetComponent<Animation>().Play();
                                    }
                                }
                                typeof(GameScript).GetMethod("RefreshStats", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
                                Network.RemoveRPCs(MenuScript.playerAppearance.GetComponent<NetworkView>().viewID);
                                MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
                                {
                            GameScript.equippedIDs,
                            0,
                            GameScript.dead
                                });
                                __instance.UpdateHP();
                                __instance.UpdateEnergy();
                                __instance.UpdateMana();
                            }
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                            ___storage[i] = ___inventory[slot];
                            ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                            typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                            __instance.StartCoroutine((IEnumerator)typeof(GameScript).GetMethod("RefreshStoragePage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ___curStoragePage }));
                            if (slotItemType == ItemType.DROID && slot > 41)
                            {
                                ___droidCount--;
                                typeof(GameScript).GetMethod("UpdateDroids", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
                            }
                            if (slotInfo != null) slotInfo.InvokeOnDequip(slot);
                            break;
                        }
                    }
                }
                ___shiftclicking = false;
                __result = FakeRoutine();
                return false;
            }
            return true;
        }

        public static IEnumerator FakeRoutine()
        {
            yield break;
        }
    }
}