using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerTrigger))]
    [HarmonyPatch("GetNPCid")]
    internal static class Patch_PlayerTrigger_GetNPCid
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerTrigger __instance, string name, ref int __result)
        {
            int id = TileRegistry.GetIDByPropName(name.Replace("(Clone)", ""));
            if (id >= 0)
            {
                __result = id;
                return false;
            }
            return true;
        }
    }
}