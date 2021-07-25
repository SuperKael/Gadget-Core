using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetAllegianceName")]
    static class Patch_GameScript_GetAllegianceName
    {
        [HarmonyPrefix]
        public static bool Prefix(int i, ref string __result)
        {
            if (AllegianceRegistry.Singleton.TryGetEntry(i, out AllegianceInfo entry))
            {
                __result = entry.Name;
                return false;
            }
            return true;
        }
    }
}