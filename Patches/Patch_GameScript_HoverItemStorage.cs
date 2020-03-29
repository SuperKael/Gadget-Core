using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemStorage")]
    static class Patch_GameScript_HoverItemStorage
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___storage, int ___curStoragePage)
        {
            int num = slot + ___curStoragePage * 30;
            PatchMethods.HoverItem(___storage[num]);
            return false;
        }
    }
}