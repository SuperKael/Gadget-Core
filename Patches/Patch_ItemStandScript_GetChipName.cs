using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ItemStandScript))]
    [HarmonyPatch("GetChipName")]
    static class Patch_ItemStandScript_GetChipName
    {
        [HarmonyPrefix]
        public static bool Prefix(ItemStandScript __instance, int id, ref string __result)
        {
            if (ChipRegistry.Singleton.HasEntry(id))
            {
                __result = ChipRegistry.Singleton.GetEntry(id).Name;
                return false;
            }
            return true;
        }
    }
}