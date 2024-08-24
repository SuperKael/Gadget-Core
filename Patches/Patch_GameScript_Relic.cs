using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Relic")]
    internal static class Patch_GameScript_Relic
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance)
        {
            if (PlanetRegistry.Singleton[SpawnerScript.curBiome] is PlanetInfo planet)
            {
                planet.Relics++;
                planet.PortalUses += 3;
                if (Menuu.curUniform == 9)
                {
                    planet.PortalUses += 3;
                }
                return false;
            }
            return true;
        }
    }
}