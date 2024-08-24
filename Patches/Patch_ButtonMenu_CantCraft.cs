using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ButtonMenu))]
    [HarmonyPatch("CantCraft")]
    internal static class Patch_ButtonMenu_CantCraft
    {
        [HarmonyPrefix]
        public static void Prefix(ButtonMenu __instance, int a)
        {
            if (MenuRegistry.Singleton.GetEntry(a) is CraftMenuInfo craftMenu)
            {
                __instance.button = (Material)Resources.Load("mat/craftButtonInactive" + craftMenu.ID);
                __instance.buttonSelect = (Material)Resources.Load("mat/craftButtonInactive" + craftMenu.ID);
                __instance.GetComponent<Renderer>().material = __instance.button;
            }
        }
    }
}