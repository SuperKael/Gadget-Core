using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetPlanetName")]
    static class Patch_GameScript_GetPlanetName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int a, ref string __result)
        {
            if (PlanetRegistry.Singleton.HasEntry(a))
            {
                __result = PlanetRegistry.Singleton.GetEntry(a).Name;
                return false;
            }
            return true;
        }
    }
}