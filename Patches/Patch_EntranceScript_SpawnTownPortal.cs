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
    [HarmonyPatch("SpawnTownPortal")]
    static class Patch_EntranceScript_SpawnTownPortal
    {
        [HarmonyPrefix]
        public static bool Prefix(EntranceScript __instance)
        {
			if (PlanetRegistry.Singleton[SpawnerScript.curBiome] is PlanetInfo planet && planet.GetEntryType() == PlanetType.SINGLE)
			{
				GameScript.endPortal[3] = (GameObject)Network.Instantiate((GameObject)Resources.Load("portal"), new Vector3(__instance.spawnSpot[3].transform.position.x, __instance.spawnSpot[3].transform.position.y + 1.2f, 0f), Quaternion.identity, 0);
				GameScript.endPortalUA[3] = GameScript.endPortal[3].transform.GetChild(0).gameObject;
				GameScript.endPortal[3].GetComponent<NetworkView>().RPC("Activate", RPCMode.All, new object[0]);
				GameScript.endPortalUA[3].GetComponent<NetworkView>().RPC("Set", RPCMode.AllBuffered, new object[]
				{
					98,
					0,
					3
				});
				return false;
			}
			return true;
        }
    }
}