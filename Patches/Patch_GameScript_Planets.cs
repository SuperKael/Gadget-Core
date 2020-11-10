using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Planets")]
    static class Patch_GameScript_Planets
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance)
        {
            PlanetRegistry.UpdatePlanetSelector();
        }
    }
}