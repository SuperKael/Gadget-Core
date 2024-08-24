using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshStats")]
    internal static class Patch_GameScript_RefreshStats
    {
        [HarmonyPostfix]
        public static void Postfix(Item[] ___inventory)
        {
            PatchMethods.RecalculateGearStats(___inventory);
        }
    }
}