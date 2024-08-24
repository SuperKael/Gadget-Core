using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Planets")]
    internal static class Patch_GameScript_Planets
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance)
        {
            PlanetRegistry.UpdatePlanetSelector();
        }
    }
}