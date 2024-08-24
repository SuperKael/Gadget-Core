using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemCraft")]
    internal static class Patch_GameScript_HoverItemCraft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___craft)
        {
            PatchMethods.HoverItem(___craft[slot]);
            return false;
        }
    }
}