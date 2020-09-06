using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ShiftClick2")]
    static class Patch_GameScript_ShiftClick2
    {
        private static Item itemInSlot;

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref bool ___shiftclicking, Item[] ___inventory, ref Item[] ___craft, ref int ___craftType, int slot, ref IEnumerator __result)
        {
            itemInSlot = ___inventory[slot];
            if (!___shiftclicking)
            {
                ItemInfo slotInfo = ItemRegistry.GetSingleton().GetEntry(itemInSlot.id);
                ItemType slotItemType = slotInfo != null ? (slotInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(itemInSlot.id);
                ItemType slotItemTypeUnfiltered = slotInfo != null ? slotInfo.Type : ItemRegistry.GetDefaultTypeByID(itemInSlot.id);
                bool flag = false;
                int num = 0;
                if (___craftType == 0)
                {
                    if ((slotItemType & ItemType.BASIC_MASK) == ItemType.EMBLEM)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (___craft[i].id == itemInSlot.id && ___craft[i].corrupted == itemInSlot.corrupted && ___craft[i].q + ___inventory[slot].q <= 9999)
                            {
                                flag = true;
                                num = i;
                                break;
                            }
                        }
                        if (flag)
                        {
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                            ___craft[num].q += ___inventory[slot].q;
                            ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                            typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                            typeof(GameScript).GetMethod("RefreshSlotCraft", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { num });
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (___craft[i].id == 0)
                                {
                                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                    ___craft[i] = ___inventory[slot];
                                    ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                    typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                                    typeof(GameScript).GetMethod("RefreshSlotCraft", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (___craftType == 1 && (slotItemType & ItemType.EMBLEM) == ItemType.LOOT && (slotItemTypeUnfiltered & ItemType.ORGANIC) == ItemType.ORGANIC)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (___craft[i].id == itemInSlot.id && ___craft[i].corrupted == itemInSlot.corrupted && ___craft[i].q + ___inventory[slot].q <= 9999)
                        {
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                            flag = true;
                            num = i;
                            break;
                        }
                    }
                    if (flag)
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                        ___craft[num].q += ___inventory[slot].q;
                        ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                        typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                        typeof(GameScript).GetMethod("RefreshSlotCraft", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { num });
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (___craft[i].id == 0)
                            {
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___craft[i] = ___inventory[slot];
                                ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                                typeof(GameScript).GetMethod("RefreshSlotCraft", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                break;
                            }
                        }
                    }
                }
                ___shiftclicking = false;
                typeof(GameScript).GetMethod("CraftCheck", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
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