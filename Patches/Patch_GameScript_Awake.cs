using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Awake")]
    static class Patch_GameScript_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance)
        {
            InstanceTracker.gameScript = __instance;
            foreach (GadgetModInfo mod in GadgetMods.ListAllModInfos())
            {
                __instance.gameObject.AddComponent<GadgetModHookScript>().Mod = mod;
            }
        }
    }
}