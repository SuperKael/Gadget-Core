using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Interact")]
    internal static class Patch_PlayerScript_Interact
    {
        public static readonly FieldInfo interacting = typeof(PlayerScript).GetField("interacting", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, int id, ref IEnumerator __result)
        {
            if (TileRegistry.GetSingleton().HasEntry(id))
            {
                TileInfo tile = TileRegistry.GetSingleton().GetEntry(id);
                IEnumerable<IEnumerator> interactions = tile.Interact();
                if (interactions != null)
                {
                    __result = InteractAll(__instance, interactions);
                    return false;
                }
            }
            return true;
        }

        private static IEnumerator InteractAll(PlayerScript instance, IEnumerable<IEnumerator> interactions)
        {
            IEnumerable<Coroutine> routines = interactions.Select(x => instance.StartCoroutine(x));
            foreach (Coroutine routine in routines) yield return routine;
            interacting.SetValue(instance, false);
        }
    }
}