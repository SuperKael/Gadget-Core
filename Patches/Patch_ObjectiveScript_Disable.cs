using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectiveScript))]
    [HarmonyPatch("Disable")]
    static class Patch_ObjectiveScript_Disable
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectiveScript __instance)
        {
            LootTables.DropLoot("object:objective", __instance.transform.position, 0.1f);
        }
    }
}