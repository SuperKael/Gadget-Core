using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetUniformUnlock")]
    internal static class Patch_Menuu_GetUniformUnlock
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterUniformRegistry.Singleton.TryGetEntry(a, out CharacterUniformInfo entry))
            {
                __result = entry.UnlockCondition;
                return false;
            }
            return true;
        }
    }
}