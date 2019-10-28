using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(WormScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_WormScript_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(WormScript __instance)
        {
            LootTables.DropLoot("entity:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }
    }
}