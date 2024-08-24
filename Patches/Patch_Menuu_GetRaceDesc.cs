using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetRaceDesc")]
    internal static class Patch_Menuu_GetRaceDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterRaceRegistry.Singleton.TryGetEntry(a, out CharacterRaceInfo entry))
            {
                __result = entry.Desc;
                return false;
            }
            return true;
        }
    }
}