using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Awake")]
    static class Patch_PlayerScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance)
        {
            if (__instance.GetComponent<NetworkView>().isMine)
            {
                InstanceTracker.PlayerScript = __instance;
                InstanceTracker.SpawnerScript = Camera.main.gameObject.GetComponent<SpawnerScript>();
            }
        }
    }
}