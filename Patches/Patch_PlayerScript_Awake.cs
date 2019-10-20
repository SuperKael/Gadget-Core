using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Awake")]
    static class Patch_PlayerScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance)
        {
            InstanceTracker.playerScript = __instance;
        }
    }
}