using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MusicBox))]
    [HarmonyPatch("Start")]
    static class Patch_MusicBox_Start
    {
        [HarmonyPrefix]
        public static void Prefix(MusicBox __instance)
        {
            InstanceTracker.musicBox = __instance;
        }
    }
}