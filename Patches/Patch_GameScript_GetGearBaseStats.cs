using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetGearBaseStats")]
    static class Patch_GameScript_GetGearBaseStats
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref int[] __result)
        {
            if (ItemRegistry.GetSingleton().HasEntry(id))
            {
                __result = ItemRegistry.GetSingleton().GetEntry(id).Stats.GetStatArray();
                return false;
            }
            return true;
        }
    }
}