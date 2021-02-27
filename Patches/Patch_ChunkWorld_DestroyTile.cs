using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("DestroyTile")]
    static class Patch_ChunkWorld_DestroyTile
    {
        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, int[] msg, GameScript ___gameScript)
        {
            int num = msg[0];
            int num2 = msg[1];
            int num3 = (num + 62) / 4;
            int num4 = (num2 + 62) / 4;
            if (__instance.grid[num3, num4] > 0)
            {
                ___gameScript.SpawnDroppedItem(TileRegistry.Singleton.TryGetEntry(__instance.grid[num3, num4], out TileInfo tile) ? tile.Item?.ID ?? 0 : __instance.grid[num3, num4], new Vector3(num, num2, 0f));
                __instance.grid[num3, num4] = 0;
                __instance.CreateVisualMesh();
            }
            __instance.MultiplayerSupport();
            return false;
        }
    }
}