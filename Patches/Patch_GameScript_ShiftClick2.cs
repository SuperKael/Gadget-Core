using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using GadgetCore.API;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;
using GadgetCore.Util;
using System;
using System.Text;

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
                ItemInfo slotInfo = ItemRegistry.GetItem(itemInSlot.id);
                ItemType slotItemTypeUnfiltered = slotInfo.Type;
                ItemType slotItemType = slotItemTypeUnfiltered & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK);
                bool flag = false;
                int num = 0;
                if ((___craftType == 0 && (slotItemType & (ItemType.EMBLEM | ItemType.EQUIPABLE)) == ItemType.EMBLEM) ||
                    (___craftType == 1 && (slotItemType & (ItemType.EMBLEM | ItemType.EQUIPABLE)) == ItemType.LOOT && (slotItemTypeUnfiltered & ItemType.ORGANIC) == ItemType.ORGANIC))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (GadgetCoreAPI.CanItemsStack(___craft[i], itemInSlot) && ___craft[i].q + ___inventory[slot].q <= 9999)
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
                        __instance.RefreshSlot(slot);
                        __instance.RefreshSlotCraft(num);
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
                                __instance.RefreshSlot(slot);
                                __instance.RefreshSlotCraft(i);
                                break;
                            }
                        }
                    }
                }
                else if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
                {
                    Item[] craftItems = ___craft;
                    HashSet<int> acceptableSlots = new HashSet<int>(Enumerable.Range(0, ___craft.Length).Where(x => craftMenu.SlotValidators.Any(v => v(___inventory[slot], craftItems, x))));
                    foreach (int i in acceptableSlots)
                    {
                        if (GadgetCoreAPI.CanItemsStack(___craft[i], itemInSlot) && ___craft[i].q < 9999)
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
                        if (___craft[num].q > 9999)
                        {
                            ___inventory[slot].q = ___craft[num].q - 9999;
                            ___craft[num].q = 9999;
                        }
                        else
                        {
                            ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                        }
                        __instance.RefreshSlot(slot);
                        __instance.RefreshSlotCraft(num);
                    }
                    else
                    {
                        foreach (int i in acceptableSlots)
                        {
                            if (___craft[i].id == 0)
                            {
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK3"), Menuu.soundLevel / 10f);
                                ___craft[i] = ___inventory[slot];
                                ___inventory[slot] = new Item(0, 0, 0, 0, 0, new int[3], new int[3]);
                                __instance.RefreshSlot(slot);
                                __instance.RefreshSlotCraft(i);
                                break;
                            }
                        }
                    }
                }
                ___shiftclicking = false;
                __instance.InvokeMethod("CraftCheck");
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