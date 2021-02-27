using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("OnDestroy")]
    static class Patch_Chunk_OnDestroy
    {
        [HarmonyPostfix]
        public static void Postfix(ref GameObject[] ___networkStuff)
        {
            if (___networkStuff.Length > 40)
            {
                if (Network.isServer)
                {
                    for (int i = 40; i < ___networkStuff.Length; i++)
                    {
                        if (___networkStuff[i])
                        {
                            Network.RemoveRPCs(___networkStuff[i].GetComponent<NetworkView>().viewID);
                            Network.Destroy(___networkStuff[i].gameObject);
                        }
                    }
                }
                ___networkStuff = new GameObject[40];
            }
        }
    }
}