using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SplitItem")]
    static class Patch_GameScript_SplitItem
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___inventory)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                __instance.SelectItem(slot);
                return false;
            }
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
            if (___inventory[slot].q > 1)
            {
                int q = ___inventory[slot].q;
                ___inventory[slot].q /= 2;
                int num = q - ___inventory[slot].q;
                ___holdingItem = GadgetCoreAPI.CopyItem(___inventory[slot]);
                ___holdingItem.q = num;
                __instance.RefreshSlot(slot);
                __instance.RefreshHoldingSlot();
            }
            return false;
        }
    }
}