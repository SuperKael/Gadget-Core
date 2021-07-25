using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;
using UnityEngine;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(EnemyScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_EnemyScript_DropLocal
    {
        private static Dictionary<EnemyScript, Vector3> pos = new Dictionary<EnemyScript, Vector3>();

        [HarmonyPrefix]
        public static void Prefix(EnemyScript __instance)
        {
            pos.Add(__instance, __instance.t.position);
        }

        [HarmonyPostfix]
        public static void Postfix(EnemyScript __instance)
        {
            LootTables.DropLoot("entity:" + __instance.transform.GetHighestParent().name.Split('(')[0], pos[__instance]);
            pos.Remove(__instance);
        }
    }
}