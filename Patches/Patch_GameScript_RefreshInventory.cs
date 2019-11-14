using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshInventory")]
    static class Patch_GameScript_RefreshInventory
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref IEnumerator __result)
        {
            __result = RefreshInventory(__instance);
            return false;
        }

        private static IEnumerator RefreshInventory(GameScript __instance)
        {
            for (int i = 0; i < 45; i++)
            {
                RefreshSlot.Invoke(__instance, new object[] { i });
            }
            yield return null;
            yield break;
        }
    }
}