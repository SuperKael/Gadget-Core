using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ShiftClickGather")]
    static class Patch_GameScript_ShiftClickGather
    {
        private static Item itemInSlot;

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref bool ___shiftclicking, Item[] ___inventory, ref Item[] ___gatherStorage, int slot, int ___curStoragePage, ref IEnumerator __result)
        {
            itemInSlot = ___gatherStorage[slot];
            if (!___shiftclicking)
            {
                ItemInfo slotInfo = ItemRegistry.GetSingleton().GetEntry(itemInSlot.id);
                ItemType slotItemType = slotInfo != null ? (slotInfo.Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(itemInSlot.id);
                int num = 0;
                if ((slotItemType & ItemType.NONSTACKING) == ItemType.STACKING)
                {
                    bool flag1 = false, flag2 = false;
                    for (int i = 0; i < 36; i++)
                    {
                        if (___inventory[i].id == itemInSlot.id && ___inventory[i].q < 9999)
                        {
                            if (___inventory[i].q + ___gatherStorage[slot].q <= 9999)
                            {
                                flag1 = true;
                                num = i;
                                break;
                            }
                            else
                            {
                                if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___gatherStorage[slot].q -= 9999 - ___inventory[i].q;
                                ___inventory[i].q = 9999;
                                typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                flag2 = true;
                            }
                        }
                    }
                    if (flag1)
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                        ___inventory[num].q += ___gatherStorage[slot].q;
                        ___gatherStorage[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                        typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { num });
                    }
                    else
                    {
                        for (int i = 0; i < 36; i++)
                        {
                            if (___inventory[i].id == 0)
                            {
                                if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___inventory[i] = ___gatherStorage[slot];
                                ___gatherStorage[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                break;
                            }
                            else if (___inventory[i].id == itemInSlot.id && ___inventory[i].q < 9999)
                            {
                                if (___inventory[i].q + ___gatherStorage[slot].q <= 9999)
                                {
                                    if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                    ___inventory[i].q += ___gatherStorage[slot].q;
                                    ___gatherStorage[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                    typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                    break;
                                }
                                else
                                {
                                    if (!flag2) __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                    ___gatherStorage[slot].q -= 9999 - ___inventory[i].q;
                                    ___inventory[i].q = 9999;
                                    typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                                }
                                flag2 = true;
                            }
                        }
                    }
                    typeof(GameScript).GetMethod("RefreshSlotGather", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                    ___shiftclicking = false;
                    __result = FakeRoutine();
                    return false;
                }
                else
                {
                    for (int i = 0; i < 36; i++)
                    {
                        if (___inventory[i].id == 0)
                        {
                            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                            ___inventory[i] = ___gatherStorage[slot];
                            ___gatherStorage[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                            typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { i });
                            typeof(GameScript).GetMethod("RefreshSlotGather", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { slot });
                            break;
                        }
                    }
                }
            }
            return true;
        }

        public static IEnumerator FakeRoutine()
        {
            yield break;
        }
    }
}