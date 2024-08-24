using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("EnterBuildMode")]
    internal static class Patch_GameScript_EnterBuildMode
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, bool ___inShip)
        {
            if (!GameScript.combatMode && Network.isServer && ___inShip)
            {
                foreach (MenuInfo menu in MenuRegistry.Singleton)
                {
                    if (menu.IsOpen) menu.CloseMenu();
                }
            }
        }
    }
}