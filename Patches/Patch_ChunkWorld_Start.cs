using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("Start")]
    static class Patch_ChunkWorld_Start
    {
        [HarmonyPostfix]
        public static void Postfix(ChunkWorld __instance)
        {
            float spriteMultiple = 1 / Mathf.Sqrt(GadgetCoreAPI.spriteSheetSize);
            __instance.GetComponent<Renderer>().material.mainTextureScale = new Vector2(spriteMultiple, spriteMultiple);
        }
    }
}