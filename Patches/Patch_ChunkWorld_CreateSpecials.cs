using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("CreateSpecials")]
    static class Patch_ChunkWorld_CreateSpecials
    {
        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, ref GameObject[,] ___gridSpecialObj)
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    if (__instance.gridSpecial[i, j] > 0)
                    {
                        if (___gridSpecialObj[i, j] == null)
                        {
                            if (TileRegistry.GetSingleton().HasEntry(__instance.gridSpecial[i, j]))
                            {
                                TileInfo tile = TileRegistry.GetSingleton().GetEntry(__instance.gridSpecial[i, j]);
                                if (tile.Type == TileType.INTERACTIVE)
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("npc/npc" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                }
                                else
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("prop/" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                }
                            }
                            else
                            {
                                if (__instance.gridSpecial[i, j] < 2400)
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("npc/npc" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                }
                                else
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("prop/" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                }
                                if (__instance.gridSpecial[i, j] != 2106 && __instance.gridSpecial[i, j] != 2107 && __instance.gridSpecial[i, j] != 2105)
                                {
                                    ___gridSpecialObj[i, j].transform.parent = __instance.transform;
                                }
                            }
                        }
                    }
                    else if (___gridSpecialObj[i, j])
                    {
                        UnityEngine.Object.Destroy(___gridSpecialObj[i, j]);
                    }
                }
            }
            return false;
        }
    }
}