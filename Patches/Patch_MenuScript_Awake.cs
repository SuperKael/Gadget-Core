using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(MenuScript))]
    [HarmonyPatch("Awake")]
    internal static class Patch_MenuScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(MenuScript __instance)
        {
            InstanceTracker.MenuScript = __instance;
        }
    }
}