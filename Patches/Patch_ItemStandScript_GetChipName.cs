using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ItemStandScript))]
    [HarmonyPatch("GetChipName")]
    internal static class Patch_ItemStandScript_GetChipName
    {
        [HarmonyPrefix]
        public static bool Prefix(ItemStandScript __instance, int id, ref string __result)
        {
            if (ChipRegistry.Singleton.TryGetEntry(id, out ChipInfo chip))
            {
                __result = chip.Name;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(int id, ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) __result = id >= ChipRegistry.Singleton.GetIDStart() ? "Missing Modded Combat Chip!" : "Invalid Combat Chip!";
        }
    }
}