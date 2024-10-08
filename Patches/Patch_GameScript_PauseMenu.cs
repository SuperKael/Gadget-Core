using GadgetCore.API;
using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PauseMenu")]
    internal static class Patch_GameScript_PauseMenu
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !GadgetCoreAPI.IsInputFrozen();
        }
    }
}