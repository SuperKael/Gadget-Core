using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PortalScript))]
    [HarmonyPatch("GetBiomeName")]
    static class Patch_PortalScript_GetBiomeName
    {
        [HarmonyPrefix]
        public static bool Prefix(PortalScript __instance, int ___biome, ref string __result)
        {
            if (PlanetRegistry.Singleton.HasEntry(___biome))
            {
                __result = PlanetRegistry.Singleton.GetEntry(___biome).Name;
                return false;
            }
            return true;
        }
    }
}