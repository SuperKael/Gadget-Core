using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Hivemind))]
    [HarmonyPatch("DropLocal")]
    static class Patch_Hivemind_DropLocal
    {
        [HarmonyPostfix]
        public static void Postfix(Hivemind __instance)
        {
            LootTables.DropLoot("entity:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }
    }
}