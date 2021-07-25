using HarmonyLib;
using GadgetCore.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GadgetCore.API.Dialog;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ExitTalking")]
    static class Patch_GameScript_ExitTalking
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DialogChains.activeChain = null;
        }
    }
}