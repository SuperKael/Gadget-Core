using GadgetCore.API;
using HarmonyLib;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("CreateCharacter")]
    static class Patch_Menuu_CreateCharacter
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            AllegianceRegistry.RebuildAllegianceArray();
        }
    }
}