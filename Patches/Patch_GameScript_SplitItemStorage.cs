
using HarmonyLib;
using GadgetCore.API;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SplitItemStorage")]
    static class Patch_GameScript_SplitItemStorage
    {
        public static readonly MethodInfo SelectItemStorage = typeof(GameScript).GetMethod("SelectItemStorage", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, ref Item ___holdingItem)
        {
            ItemInfo itemInfo = ItemRegistry.Singleton.GetEntry(___holdingItem.id);
            ItemType holdingItemType = itemInfo != null ? (itemInfo.Type & (ItemType.EQUIP_MASK | ItemType.TYPE_MASK)) : ItemRegistry.GetDefaultTypeByID(___holdingItem.id);
            if ((holdingItemType & ItemType.NONSTACKING) == ItemType.NONSTACKING)
            {
                SelectItemStorage.Invoke(__instance, new object[] { slot });
                return false;
            }
            return true;
        }
    }
}