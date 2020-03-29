using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CloseAllMenus")]
    static class Patch_GameScript_CloseAllMenus
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance)
        {
            if (GadgetCoreAPI.menus != null)
            {
                foreach (GameObject menu in GadgetCoreAPI.menus)
                {
                    menu.SendMessage("OnMenuClosed", options: SendMessageOptions.DontRequireReceiver);
                    menu.SetActive(false);
                }
            }
        }
    }
}