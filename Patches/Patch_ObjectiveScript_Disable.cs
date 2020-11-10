using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectiveScript))]
    [HarmonyPatch("Disable")]
    static class Patch_ObjectiveScript_Disable
    {
        [HarmonyPostfix]
        public static void Postfix(ObjectiveScript __instance)
        {
            LootTables.DropLoot("special:objective", new Vector3(__instance.transform.position.x, __instance.transform.position.y, 0f), 0.1f);
        }
    }
}