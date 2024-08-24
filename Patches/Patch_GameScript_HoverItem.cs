using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItem")]
    internal static class Patch_GameScript_HoverItem
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___inventory)
        {
            PatchMethods.HoverItem(___inventory[slot]);
            return false;
        }
    }
}