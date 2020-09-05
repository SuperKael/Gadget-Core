using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("EXPGEAR")]
    static class Patch_GameScript_EXPGEAR
    {
        [HarmonyPostfix]
        public static void Postfix(Item[] ___inventory)
        {
            PatchMethods.RecalculateGearStats(___inventory);
        }
    }
}