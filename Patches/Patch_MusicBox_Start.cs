using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MusicBox))]
    [HarmonyPatch("Start")]
    internal static class Patch_MusicBox_Start
    {
        [HarmonyPrefix]
        public static void Prefix(MusicBox __instance)
        {
            InstanceTracker.MusicBox = __instance;
        }
    }
}