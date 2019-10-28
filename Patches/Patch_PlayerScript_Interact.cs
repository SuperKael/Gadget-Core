using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Interact")]
    static class Patch_PlayerScript_Interact
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerTrigger __instance, int id, ref IEnumerator __result)
        {
            if (TileRegistry.GetSingleton().HasEntry(id))
            {
                TileInfo tile = TileRegistry.GetSingleton().GetEntry(id);
                IEnumerator result = tile.Interact();
                if (result != null)
                {
                    __result = result;
                    return false;
                }
            }
            return true;
        }
    }
}