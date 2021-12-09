using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetUniformDesc")]
    static class Patch_Menuu_GetUniformDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterUniformRegistry.Singleton.TryGetEntry(a, out CharacterUniformInfo entry))
            {
                __result = entry.Desc;
                return false;
            }
            return true;
        }
    }
}