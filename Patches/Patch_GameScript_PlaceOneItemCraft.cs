using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlaceOneItemCraft")]
    static class Patch_GameScript_PlaceOneItemCraft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem, ref Item[] ___craft)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING && ___craft[slot].id == ___holdingItem.id)
            {
                return false;
            }
            __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK2"), Menuu.soundLevel / 10f);
            if (___craft[slot].id == ___holdingItem.id)
            {
                ___craft[slot].q++;
            }
            else
            {
                ___craft[slot] = GadgetCoreAPI.CopyItem(___holdingItem);
                ___craft[slot].q = 1;
            }
            ___holdingItem.q--;
            __instance.RefreshSlotCraft(slot);
            __instance.RefreshHoldingSlot();
            __instance.CraftCheck();
            return false;
        }
    }
}