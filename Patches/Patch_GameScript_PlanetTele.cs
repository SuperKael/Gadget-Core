using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlanetTele")]
    static class Patch_GameScript_PlanetTele
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref bool ___planetTeleing, ref int[] ___portalUses)
        {
			int curPlanet = (PlanetRegistry.PlanetSelectorPage - 1) * 14 + GameScript.curPlanet;
			if (!___planetTeleing && curPlanet >= 0 && curPlanet - 14 < PlanetRegistry.selectorPlanets.Length && GameScript.curPlanetTrue != curPlanet)
			{
				if (PlanetRegistry.PlanetSelectorPage > 1)
				{
					if (PlanetRegistry.selectorPlanets[curPlanet - 14] is PlanetInfo planet)
					{
						__instance.InvokeMethod("RefreshChallengeButton");
						Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
						Object.Instantiate(Resources.Load("clickBurst"), new Vector3(vector.x, vector.y, 0f), Quaternion.identity);
						___planetTeleing = true;
						if (Network.isServer)
						{
							__instance.InvokeMethod("ChangePortal", planet.ID, GameScript.challengeLevel);
						}
						else
						{
							__instance.GetComponent<NetworkView>().RPC("ChangePortal", RPCMode.Server, new object[]
							{
							planet.ID,
							GameScript.challengeLevel
							});
						}
						if (planet.PortalUses != -1)
						{
							planet.PortalUses--;
						}
						GameScript.curPlanetTrue = curPlanet;
						__instance.Planets();
						__instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("PlanetTel"));
					}
				}
				else
				{
					__instance.InvokeMethod("RefreshChallengeButton");
					Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					Object.Instantiate(Resources.Load("clickBurst"), new Vector3(vector.x, vector.y, 0f), Quaternion.identity);
					___planetTeleing = true;
					if (Network.isServer)
					{
						__instance.InvokeMethod("ChangePortal", GameScript.curPlanet, GameScript.challengeLevel);
					}
					else
					{
						__instance.GetComponent<NetworkView>().RPC("ChangePortal", RPCMode.Server, new object[]
						{
							GameScript.curPlanet,
							GameScript.challengeLevel
						});
					}
					if (___portalUses[GameScript.curPlanet] != -1)
					{
						___portalUses[GameScript.curPlanet]--;
						PreviewLabs.PlayerPrefs.SetInt("portalUses" + GameScript.curPlanet, ___portalUses[GameScript.curPlanet]);
					}
					GameScript.curPlanetTrue = GameScript.curPlanet;
					__instance.Planets();
					__instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("PlanetTel"));
				}
			}
			return false;
		}
    }
}