using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(RuneChalice))]
    [HarmonyPatch("GetItemName")]
    static class Patch_RuneChalice_GetItemName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.GetSingleton().HasEntry(id))
            {
                __result = ItemRegistry.GetSingleton().GetEntry(id).Name;
                return false;
            }
            return true;
        }
    }
}