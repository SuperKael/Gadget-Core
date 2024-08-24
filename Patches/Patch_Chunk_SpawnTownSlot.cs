using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("SpawnTownSlot")]
    internal static class Patch_Chunk_SpawnTownSlot
    {
        [HarmonyPrefix]
        public static bool Prefix(Chunk __instance, int a, int i, int mid, ref GameObject[] ___networkStuff, ref int ___temp)
        {
            if (___temp >= ___networkStuff.Length)
            {
                GameObject[] newNetworkStuff = new GameObject[___networkStuff.Length * 2];
                Array.Copy(___networkStuff, newNetworkStuff, ___networkStuff.Length);
                ___networkStuff = newNetworkStuff;
            }
            if (PlanetRegistry.Singleton[a] is PlanetInfo planet)
            {
                Transform transform = __instance.spawnSpot[i].transform;
                if (mid == 1)
                {
                    transform = __instance.spawnSpotMid[i].transform;
                }
                IEnumerable<GameObject> objs = planet.InvokeOnSpawnTownSlot(__instance, transform.position);
                if (objs != null)
                {
                    foreach (GameObject obj in objs)
                    {
                        if (obj == null) continue;
                        if (___temp >= ___networkStuff.Length)
                        {
                            GameObject[] newNetworkStuff = new GameObject[___networkStuff.Length * 2];
                            Array.Copy(___networkStuff, newNetworkStuff, ___networkStuff.Length);
                            ___networkStuff = newNetworkStuff;
                        }
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