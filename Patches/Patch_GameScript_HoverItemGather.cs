using UnityEngine;
using HarmonyLib;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemGather")]
    static class Patch_GameScript_HoverItemGather
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___gatherStorage)
        {
            PatchMethods.HoverItem(___gatherStorage[slot]);
            return false;
        }
    }
}