using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("GetUniformName")]
    static class Patch_Menuu_GetUniformName
    {
        [HarmonyPrefix]
        public static bool Prefix(int a, ref string __result)
        {
            if (CharacterUniformRegistry.Singleton.TryGetEntry(a, out CharacterUniformInfo entry))
            {
                __result = entry.Name;
                return false;
            }
            return true;
        }
    }
}