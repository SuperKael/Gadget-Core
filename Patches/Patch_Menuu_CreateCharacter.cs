using GadgetCore.API;
using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("CreateCharacter")]
    internal static class Patch_Menuu_CreateCharacter
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            AllegianceRegistry.RebuildAllegianceArray();
        }
    }
}