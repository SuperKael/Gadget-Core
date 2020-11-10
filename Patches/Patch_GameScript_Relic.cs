using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Relic")]
    static class Patch_GameScript_Relic
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