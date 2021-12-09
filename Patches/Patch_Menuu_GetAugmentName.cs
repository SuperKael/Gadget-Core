using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetAugmentName")]
    static class Patch_Menuu_GetAugmentName
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterAugmentRegistry.Singleton.TryGetEntry(a, out CharacterAugmentInfo entry))
            {
                __result = entry.Name;
                return false;
            }
            return true;
        }
    }
}