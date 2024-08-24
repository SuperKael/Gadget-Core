using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CloseAllMenus")]
    internal static class Patch_GameScript_CloseAllMenus
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance)
        {
            foreach (MenuInfo menu in MenuRegistry.Singleton)
            {
                menu.CloseMenu();
            }
        }
    }
}