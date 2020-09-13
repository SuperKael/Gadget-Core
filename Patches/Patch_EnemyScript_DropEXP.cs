using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(EnemyScript))]
    [HarmonyPatch("DropEXP")]
    static class Patch_EnemyScript_DropEXP
    {
        [HarmonyPrefix]
        public static bool Prefix(EnemyScript __instance, ref int ___exp)
        {
            ___exp = (int)(___exp * (1f + GameScript.MODS[23] * 0.05f));
            GadgetCoreAPI.SpawnExp(__instance.t.position, ___exp);
            return false;
        }
    }
}