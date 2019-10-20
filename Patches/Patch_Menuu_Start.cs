using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("Start")]
    static class Patch_Menuu_Start
    {
        [HarmonyPrefix]
        public static void Prefix(Menuu __instance)
        {
            InstanceTracker.menuu = __instance;
        }
    }
}