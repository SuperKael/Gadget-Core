using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetItemWorth")]
    static class Patch_GameScript_GetItemWorth
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref int __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = ItemRegistry.Singleton.GetEntry(id).GetValue();
                return false;
            }
            return true;
        }
    }
}