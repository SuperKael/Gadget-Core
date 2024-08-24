using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemGather")]
    internal static class Patch_GameScript_HoverItemGather
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___gatherStorage)
        {
            PatchMethods.HoverItem(___gatherStorage[slot]);
            return false;
        }
    }
}