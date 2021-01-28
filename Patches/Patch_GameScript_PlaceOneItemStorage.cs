using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlaceOneItemStorage")]
    static class Patch_GameScript_PlaceOneItemStorage
    {
        public static readonly MethodInfo SwapItemStorage = typeof(GameScript).GetMethod("SwapItemStorage", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                SwapItemStorage.Invoke(__instance, new object[] { slot });
                return false;
            }
            return true;
        }
    }
}