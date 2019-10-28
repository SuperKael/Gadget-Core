using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CombineItem")]
    static class Patch_GameScript_CombineItem
    {
        public static readonly MethodInfo SwapItem = typeof(GameScript).GetMethod("SwapItem", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int slot, Item[] ___inventory)
        {
            Item item = ___inventory[slot];
            if (ItemRegistry.GetSingleton().HasEntry(item.id))
            {
                ItemInfo info = ItemRegistry.GetSingleton().GetEntry(item.id);
                if ((info.Type & ItemType.NONSTACKING) == ItemType.NONSTACKING)
                {
                    SwapItem.Invoke(__instance, new object[] { slot });
                    return false;
                }
            }
            return true;
        }
    }
}