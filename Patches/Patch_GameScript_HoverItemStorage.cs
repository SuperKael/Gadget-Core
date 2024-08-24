using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemStorage")]
    internal static class Patch_GameScript_HoverItemStorage
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___storage, int ___curStoragePage)
        {
            PatchMethods.HoverItem(___storage[slot + ___curStoragePage * 30]);
            return false;
        }
    }
}