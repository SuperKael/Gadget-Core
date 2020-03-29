using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetChipDesc")]
    static class Patch_GameScript_GetChipDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ChipRegistry.GetSingleton().HasEntry(id))
            {
                __result = ChipRegistry.GetSingleton().GetEntry(id).Desc;
                return false;
            }
            return true;
        }
    }
}