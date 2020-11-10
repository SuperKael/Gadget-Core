using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GadgetCore.Util;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(EntranceScript))]
    [HarmonyPatch("GetPotentialBiome")]
    static class Patch_EntranceScript_GetPotentialBiome
    {
        [HarmonyPrefix]
        public static bool Prefix(int b, ref int __result)
        {
            if (b < PlanetRegistry.VanillaWeightedExitPortalIDs.Length)
            {
                if (PlanetRegistry.Singleton.HasEntry(b))
                {
                    PlanetInfo planet = PlanetRegistry.Singleton[b];
                    int[] weightedExitPortals = planet.WeightedExitPortalIDs.Union(PlanetRegistry.GetVanillaExitPortalWeights(b).Where(x => x.Item3 != -1).Select(x => Tuple.Create(x.Item1, x.Item2)), new ComparisonComparer<Tuple<int, int>>((p1, p2) => p1.Item1 - p2.Item1)).SelectMany(x => Enumerable.Repeat(x.Item1, x.Item2)).ToArray();
                    __result = weightedExitPortals[Random.Range(0, weightedExitPortals.Length)];
                }
                else
                {
                    int[] weightedExitPortals = PlanetRegistry.GetVanillaExitPortalWeights(b).SelectMany(x => Enumerable.Repeat(x.Item1, x.Item2)).ToArray();
                    __result = weightedExitPortals[Random.Range(0, weightedExitPortals.Length)];
                }
                return false;
            }
            else if (PlanetRegistry.Singleton.HasEntry(b))
            {
                PlanetInfo planet = PlanetRegistry.Singleton[b];
                int[] weightedExitPortals = planet.WeightedExitPortalIDs.SelectMany(x => Enumerable.Repeat(x.Item1, x.Item2)).ToArray();
                __result = weightedExitPortals[Random.Range(0, weightedExitPortals.Length)];
                return false;
            }
            return true;
        }
    }
}