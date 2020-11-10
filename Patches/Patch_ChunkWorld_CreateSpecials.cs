using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

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
                            if (TileRegistry.Singleton.HasEntry(__instance.gridSpecial[i, j]))
                            {
                                TileInfo tile = TileRegistry.Singleton.GetEntry(__instance.gridSpecial[i, j]);
                                try
                                {
                                    if (tile.Type == TileType.INTERACTIVE)
                                    {
                                        ___gridSpecialObj[i, j] = (GameObject)Object.Instantiate(Resources.Load("npc/npc" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                    }
                                    else
                                    {
                                        ___gridSpecialObj[i, j] = (GameObject)Object.Instantiate(Resources.Load("prop/" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                    }
                                }
                                catch (System.ArgumentException)
                                {
                                    GadgetCore.CoreLogger.LogError("Error: The Tile '" + tile.RegistryName + "' has an invalid prop!");
                                }
                            }
                            else
                            {
                                if (__instance.gridSpecial[i, j] < 2400)
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)Object.Instantiate(Resources.Load("npc/npc" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
                                }
                                else
                                {
                                    ___gridSpecialObj[i, j] = (GameObject)Object.Instantiate(Resources.Load("prop/" + __instance.gridSpecial[i, j]), new Vector3((float)(i * 4 - 62), (float)(j * 4 - 62), 0.9f), Quaternion.identity);
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
                        Object.Destroy(___gridSpecialObj[i, j]);
                    }
                }
            }
            return false;
        }
    }
}