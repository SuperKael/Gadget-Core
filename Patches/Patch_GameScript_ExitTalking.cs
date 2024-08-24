using HarmonyLib;
using GadgetCore.API.Dialog;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ExitTalking")]
    internal static class Patch_GameScript_ExitTalking
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DialogChains.activeChain = null;
        }
    }
}