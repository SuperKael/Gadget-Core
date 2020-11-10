using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("SpawnBiomeSlot")]
    static class Patch_Chunk_SpawnBiomeSlot
    {
        [HarmonyPrefix]
        public static bool Prefix(Chunk __instance, int a, int i, int mid, ref GameObject[] ___networkStuff, ref int ___temp)
        {
            if (PlanetRegistry.Singleton[a] is PlanetInfo planet)
            {
                Transform transform = __instance.spawnSpot[i].transform;
                if (mid == 1)
                {
                    transform = __instance.spawnSpotMid[i].transform;
                }
                IEnumerable<GameObject> objs = planet.InvokeOnSpawnBiomeSlot(__instance, transform.position);
                if (objs != null)
                {
                    foreach (GameObject obj in objs)
                    {
                        if (obj == null) continue;
                        ___networkStuff[___temp] = obj;
                        ___temp++;
                    }
                }
                return false;
            }
            return true;
        }
    }
}