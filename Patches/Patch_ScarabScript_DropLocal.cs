using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ScarabScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_ScarabScript_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(ScarabScript __instance)
        {
            LootTables.DropLoot("entity:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }
    }
}