using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RecipeCraftedAlready")]
    static class Patch_GameScript_RecipeCraftedAlready
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int a, int q, int ___craftType, ref bool __result)
        {
            if (___craftType >= 0 && ___craftType <= 3 && a >= ItemRegistry.Singleton.GetIDStart())
            {
                __result = GadgetCoreAPI.unlockedVanillaStationRecipes[___craftType].Contains(a);
                return false;
            }
            return true;
        }
    }
}