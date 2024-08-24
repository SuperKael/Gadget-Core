using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlaceOneItemStorage")]
    internal static class Patch_GameScript_PlaceOneItemStorage
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___storage, ref int ___curStoragePage)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                __instance.SwapItemStorage(slot);
                return false;
            }
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
            int num = slot + 30 * ___curStoragePage;
            if (___storage[num].id == ___holdingItem.id)
            {
                ___storage[num].q++;
            }
            else
            {
                ___storage[num] = GadgetCoreAPI.CopyItem(___holdingItem);
                ___storage[num].q = 1;
            }
            ___holdingItem.q--;
            __instance.RefreshSlotStorage(slot);
            __instance.RefreshHoldingSlot();
            return false;
        }
    }
}