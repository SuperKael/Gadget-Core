using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetRaceName")]
    internal static class Patch_Menuu_GetRaceName
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterRaceRegistry.Singleton.TryGetEntry(a, out CharacterRaceInfo entry))
            {
                __result = entry.Name;
                return false;
            }
            return true;
        }
    }
}