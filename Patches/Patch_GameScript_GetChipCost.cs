using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetChipCost")]
    static class Patch_GameScript_GetChipCost
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref int __result)
        {
            if (ChipRegistry.GetSingleton().HasEntry(id))
            {
                __result = ChipRegistry.GetSingleton().GetEntry(id).Cost;
                return false;
            }
            return true;
        }
    }
}