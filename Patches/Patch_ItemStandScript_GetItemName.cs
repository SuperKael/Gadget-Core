using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ItemStandScript))]
    [HarmonyPatch("GetItemName")]
    static class Patch_ItemStandScript_GetItemName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.Singleton.TryGetEntry(id, out ItemInfo item))
            {
                __result = item.Name;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(int id, ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) __result = id >= ItemRegistry.Singleton.GetIDStart() ? "Missing Mod Item!" : "Invalid Item!";
        }
    }
}