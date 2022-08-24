using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Loader;

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

                __instance.GetComponentInChildren<AudioSource>().PlayOneShot(GadgetCoreAPI.LoadAudioClipInternal("Sounds/particleacc.wav", GadgetMods.GetModByName("Tiers+")), 1);
            }
        }
    }
}