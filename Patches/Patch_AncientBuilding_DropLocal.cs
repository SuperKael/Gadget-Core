using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(AncientBuilding))]
    [HarmonyPatch("DropLocal")]
    static class Patch_AncientBuilding_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(AncientBuilding __instance)
        {
            LootTables.DropLoot("object:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }
    }
}