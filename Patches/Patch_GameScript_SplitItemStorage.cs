
using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SplitItemStorage")]
    static class Patch_GameScript_SplitItemStorage
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___storage, ref int ___curStoragePage)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                __instance.SelectItemStorage(slot);
                return false;
            }
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
            int num = slot + 30 * ___curStoragePage;
            if (___storage[num].q > 1)
            {
                int q = ___storage[num].q;
                ___storage[num].q /= 2;
                int num2 = q - ___storage[num].q;
                ___holdingItem = GadgetCoreAPI.CopyItem(___storage[num]);
                ___holdingItem.q = num2;
                __instance.RefreshSlotStorage(slot);
                __instance.RefreshHoldingSlot();
            }
            return false;
        }
    }
}