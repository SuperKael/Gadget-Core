using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetGearCrafted")]
    internal static class Patch_GameScript_GetGearCrafted
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Item[] ___craft, ref Item __result)
        {
            if (GadgetCoreAPI.gearForgeRecipes.TryGetValue(Tuple.Create(___craft[0].id, ___craft[1].id, ___craft[2].id), out Tuple<Item, int> recipe))
            {
                __result = GadgetCoreAPI.CopyItem(recipe.Item1);
                __result.q += Random.Range(0, recipe.Item2 + 1);
                return false;
            }
            return true;
        }
    }
}