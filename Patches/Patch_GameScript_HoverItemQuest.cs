using UnityEngine;
using HarmonyLib;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverItemQuest")]
    static class Patch_GameScript_HoverItemQuest
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___inventoryQuest)
        {
            PatchMethods.HoverItem(___inventoryQuest[slot]);
            return false;
        }
    }
}