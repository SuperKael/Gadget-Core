using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ButtonMenu))]
    [HarmonyPatch("CanCraft")]
    internal static class Patch_ButtonMenu_CanCraft
    {
        [HarmonyPrefix]
        public static void Prefix(ButtonMenu __instance, int a)
        {
            if (MenuRegistry.Singleton.GetEntry(a) is CraftMenuInfo craftMenu)
            {
                __instance.button = (Material)Resources.Load("mat/craftButtonActive" + craftMenu.ID);
                __instance.buttonSelect = (Material)Resources.Load("mat/craftButtonSelect" + craftMenu.ID);
                __instance.GetComponent<Renderer>().material = __instance.button;
            }
        }
    }
}