using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetRaceUnlock")]
    internal static class Patch_Menuu_GetRaceUnlock
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterRaceRegistry.Singleton.TryGetEntry(a, out CharacterRaceInfo entry))
            {
                __result = entry.UnlockCondition;
                return false;
            }
            return true;
        }
    }
}