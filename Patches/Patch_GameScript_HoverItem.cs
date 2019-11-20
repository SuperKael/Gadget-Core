using UnityEngine;
using HarmonyLib;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItem")]
    static class Patch_GameScript_HoverItem
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___inventory)
        {
            PatchMethods.HoverItem(___inventory[slot]);
            return false;
        }
    }
}