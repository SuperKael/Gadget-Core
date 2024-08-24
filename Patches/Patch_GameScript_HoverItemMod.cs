using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemMod")]
    internal static class Patch_GameScript_HoverItemMod
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___modSlot)
        {
            PatchMethods.HoverItem(___modSlot[slot]);
            return false;
        }
    }
}