using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("EXPGEAR")]
    static class Patch_GameScript_EXPGEAR
    {
        [HarmonyPostfix]
        public static void Postfix(Item[] ___inventory)
        {
            PatchMethods.RecalculateGearStats(___inventory);
        }
    }
}