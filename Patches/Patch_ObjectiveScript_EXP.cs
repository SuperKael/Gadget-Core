using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ObjectiveScript))]
    [HarmonyPatch("EXP")]
    static class Patch_ObjectiveScript_EXP
    {
        [HarmonyPrefix]
        public static bool Prefix(ObjectiveScript __instance, ref IEnumerator __result)
        {
            __result = EXP(__instance);
            return false;
        }

        private static IEnumerator EXP(ObjectiveScript __instance)
        {
            int exp = Random.Range(10, 50) + GameScript.challengeLevel * 255;
            GadgetCoreAPI.SpawnExp(new Vector3(__instance.transform.position.x, __instance.transform.position.y, 0f), exp, 0.2f);
            yield break;
        }
    }
}