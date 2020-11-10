using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch("World")]
    static class Patch_SpawnerScript_World
    {
        [HarmonyPrefix]
        public static bool Prefix(SpawnerScript __instance, int[] s, ref IEnumerator __result)
        {
            PlanetRegistry.PlanetSelectorPage = 1;
            if (PlanetRegistry.Singleton[s[0]] is PlanetInfo planet && planet.GetEntryType() == PlanetType.SPECIAL)
            {
                __result = World(__instance, s, planet);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(SpawnerScript __instance, int[] s, ref IEnumerator __result)
        {
            if (PlanetRegistry.Singleton[s[0]] is PlanetInfo planet && planet.GetEntryType() != PlanetType.SPECIAL)
            {
                __result = WorldWrapper(__result, __instance, s, planet);
            }
        }

        private static IEnumerator WorldWrapper(IEnumerator worldRoutine, SpawnerScript instance, int[] s, PlanetInfo planet)
        {
            yield return worldRoutine;
            planet.InvokeOnGenerateWorld(instance, s);
            yield break;
        }

        private static IEnumerator World(SpawnerScript instance, int[] s, PlanetInfo planet)
        {
            if (GameScript.districtLevel == 0)
            { 
                GameScript.districtLevel = 1;
            }
            MenuScript.player.GetComponent<NetworkView>().RPC("Invis", RPCMode.All, new object[0]);
            InstanceTracker.GameScript.fadeObj.SendMessage("fadeOut2");
            yield return new WaitForSeconds(0.5f);
            MenuScript.player.SendMessage("Reset");
            GameScript.inInstance = false;
            instance.ship.SetActive(false);
            SpawnerScript.curBiome = s[0];
            SpawnerScript.lastBiome = s[0];
            planet.InvokeOnGenerateWorld(instance, s);
            InstanceTracker.GameScript.TeleportPlayer(0);
            yield return new WaitForSeconds(0.8f);
            instance.musicbox.SendMessage("PlayTune", SpawnerScript.curBiome);
            InstanceTracker.GameScript.fadeObj.SendMessage("fadeIn2");
            GameScript.wormBossCounter = 0;
            GameScript.hivemindCounter = 0;
            yield break;
        }
    }
}