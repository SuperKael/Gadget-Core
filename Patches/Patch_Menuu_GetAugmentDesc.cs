using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetAugmentDesc")]
    internal static class Patch_Menuu_GetAugmentDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterAugmentRegistry.Singleton.TryGetEntry(a, out CharacterAugmentInfo entry))
            {
                __result = entry.Desc;
                return false;
            }
            return true;
        }
    }
}