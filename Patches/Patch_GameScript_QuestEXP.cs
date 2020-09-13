using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("QuestEXP")]
    static class Patch_GameScript_QuestEXP
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int tier, ref IEnumerator __result)
        {
            __result = QuestEXP(__instance, tier);
            return false;
        }

        private static IEnumerator QuestEXP(GameScript __instance, int tier)
        {
            int exp = 100 + tier * 250;
            Vector3 pos = new Vector3(MenuScript.player.transform.position.x + 5f, MenuScript.player.transform.position.y, 0f);
            GadgetCoreAPI.SpawnExp(pos, exp, 0.1f);
            yield break;
        }
    }
}