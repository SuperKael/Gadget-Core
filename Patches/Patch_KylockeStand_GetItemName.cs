using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(KylockeStand))]
    [HarmonyPatch("GetItemName")]
    static class Patch_KylockeStand_GetItemName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = ItemRegistry.Singleton.GetEntry(id).Name;
                return false;
            }
            return true;
        }
    }
}