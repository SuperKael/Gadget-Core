using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetEmblemID")]
    static class Patch_GameScript_GetEmblemID
    {
        [HarmonyPostfix]
        public static bool Prefix(int id, ref int __result)
        {
            if (GadgetCoreAPI.emblemForgeRecipes.TryGetValue(id, out Tuple<int, int> recipe))
            {
                __result = recipe.Item1;
                return false;
            }
            return true;
        }
    }
}