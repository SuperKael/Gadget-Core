using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("LeaveNPC")]
    static class Patch_GameScript_LeaveNPC
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance)
        {
            foreach (MenuInfo menu in MenuRegistry.Singleton)
            {
                if (menu.IsOpen) menu.CloseMenu();
            }
        }
    }
}