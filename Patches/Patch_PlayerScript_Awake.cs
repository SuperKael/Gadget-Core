using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Awake")]
    static class Patch_PlayerScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance)
        {
            if (InstanceTracker.PlayerScript == null) InstanceTracker.PlayerScript = __instance;
        }
    }
}