using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetGearCrafted")]
    static class Patch_GameScript_GetGearCrafted
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Item[] ___craft, ref Item __result)
        {
            if (GadgetCoreAPI.gearForgeRecipes.TryGetValue(Tuple.Create(___craft[0].id, ___craft[1].id, ___craft[2].id), out Tuple<Item, int> recipe))
            {
                __result = new Item(recipe.Item1.id, recipe.Item1.q + Random.Range(0, recipe.Item2 + 1), recipe.Item1.exp, recipe.Item1.tier, recipe.Item1.corrupted, recipe.Item1.aspect, recipe.Item1.aspectLvl);
                return false;
            }
            return true;
        }
    }
}