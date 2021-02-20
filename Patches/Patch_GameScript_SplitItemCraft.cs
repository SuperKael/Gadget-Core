using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SplitItemCraft")]
    static class Patch_GameScript_SplitItemCraft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___craft)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                __instance.SelectItemCraft(slot);
                return false;
            }
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
            if (___craft[slot].q > 1)
            {
                int q = ___craft[slot].q;
                ___craft[slot].q /= 2;
                int num = q - ___craft[slot].q;
                ___holdingItem = GadgetCoreAPI.CopyItem(___craft[slot]);
                ___holdingItem.q = num;
                __instance.RefreshSlotCraft(slot);
                __instance.RefreshHoldingSlot();
                __instance.CraftCheck();
            }
            return false;
        }
    }
}