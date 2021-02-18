using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetUltimateCrafted")]
    static class Patch_GameScript_GetUltimateCrafted
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Item[] ___craft, ref Item __result)
        {
            if (GadgetCoreAPI.ultimateForgeRecipes.TryGetValue(Tuple.Create(___craft[0].id, ___craft[2].id), out int outputID))
            {
                __result = GadgetCoreAPI.CopyItem(___craft[0]);
                __result.q = 1;
                __result.exp = 0;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(ref Item[] ___craft, ref Item __result)
        {
            __result.corrupted = ___craft[0].corrupted;
            __result.SetAllExtraData(___craft[0].GetAllExtraData());
        }
    }
}