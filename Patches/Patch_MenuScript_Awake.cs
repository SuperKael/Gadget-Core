using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("Awake")]
    static class Patch_MenuScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(MenuScript __instance)
        {
            InstanceTracker.menuScript = __instance;
        }
    }
}