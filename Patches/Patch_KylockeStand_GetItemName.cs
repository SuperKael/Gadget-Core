using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(KylockeStand))]
    [HarmonyPatch("GetItemName")]
    internal static class Patch_KylockeStand_GetItemName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = ItemRegistry.Singleton.GetEntry(id).Name;
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