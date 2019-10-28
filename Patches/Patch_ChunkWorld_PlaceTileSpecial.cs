using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("PlaceTileSpecial")]
    static class Patch_ChunkWorld_PlaceTileSpecial
    {
        public static readonly MethodInfo MultiplayerSupport = typeof(ChunkWorld).GetMethod("MultiplayerSupport", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, int[] msg, ref GameObject[,] ___gridSpecialObj)
        {
            if (TileRegistry.GetSingleton().HasEntry(msg[2]))
            {
                TileInfo tile = TileRegistry.GetSingleton().GetEntry(msg[2]);
                int num = msg[0];
                int num2 = msg[1];
                int num3 = (num + 62) / 4;
                int num4 = (num2 + 62) / 4;
                int num5 = msg[2];
                __instance.gridSpecial[num3, num4] = num5;
                if (tile.Type != TileType.INTERACTIVE)
                {
                    ___gridSpecialObj[num3, num4] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("prop/" + num5), new Vector3(num, num2, 0.9f), Quaternion.identity);
                }
                else
                {
                    ___gridSpecialObj[num3, num4] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("npc/npc" + num5), new Vector3(num, num2, 0.9f), Quaternion.identity);
                }
                ___gridSpecialObj[num3, num4].transform.parent = __instance.transform;
                MultiplayerSupport.Invoke(__instance, new object[] { });
                return false;
            }
            return true;
        }
    }
}