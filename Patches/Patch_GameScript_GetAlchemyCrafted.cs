using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetAlchemyCrafted")]
    static class Patch_GameScript_GetAlchemyCrafted
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Item[] ___craft, ref Item __result)
        {
            if (GadgetCoreAPI.alchemyStationRecipes.TryGetValue(Tuple.Create(___craft[0].id, ___craft[1].id, ___craft[2].id), out Tuple<Item, int> recipe))
            {
                __result = new Item(recipe.Item1.id, recipe.Item1.q + Random.Range(0, recipe.Item2 + 1), recipe.Item1.exp, recipe.Item1.tier, recipe.Item1.corrupted, recipe.Item1.aspect, recipe.Item1.aspectLvl);
                return false;
            }
            return true;
        }
    }
}