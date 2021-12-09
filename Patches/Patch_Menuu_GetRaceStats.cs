using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetRaceStats")]
    static class Patch_Menuu_GetRaceStats
    {
        [HarmonyPrefix]
        public static bool Prefix(int i, ref int[] __result)
        {
            if (CharacterRaceRegistry.Singleton.TryGetEntry(i, out CharacterRaceInfo entry))
            {
                __result = entry.RaceStats.GetStatArray();
                for (int j = 0; j < __result.Length; j++)
                {
                    __result[j] += 2;
                }
                return false;
            }
            return true;
        }
    }
}