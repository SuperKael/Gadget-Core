using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetChipCost")]
    static class Patch_GameScript_GetChipCost
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref int __result)
        {
            if (ChipRegistry.Singleton.HasEntry(id))
            {
                __result = ChipRegistry.Singleton.GetEntry(id).Cost;
                return false;
            }
            return true;
        }
    }
}