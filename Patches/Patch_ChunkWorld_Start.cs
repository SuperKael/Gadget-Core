using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("Start")]
    internal static class Patch_ChunkWorld_Start
    {
        [HarmonyPostfix]
        public static void Postfix(ChunkWorld __instance)
        {
            float spriteMultiple = 1 / Mathf.Sqrt(GadgetCoreAPI.spriteSheetSize);
            __instance.GetComponent<Renderer>().material.mainTextureScale = new Vector2(spriteMultiple, spriteMultiple);
        }
    }
}