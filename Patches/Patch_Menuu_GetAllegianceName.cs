using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetAllegianceName")]
    internal static class Patch_Menuu_GetAllegianceName
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