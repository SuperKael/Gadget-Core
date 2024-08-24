using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetUniformName")]
    internal static class Patch_GameScript_GetUniformName
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