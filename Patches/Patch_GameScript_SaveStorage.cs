using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SaveStorage")]
    static class Patch_GameScript_SaveStorage
    {
        [HarmonyPostfix]
        public static void Postfix(ref Item[] ___storage)
        {
            for (int i = 0; i < ___storage.Length; i++)
            {
                if (___storage[i] != null) PreviewLabs.PlayerPrefs.SetString(string.Concat("storage", i, "extraData"), ___storage[i].SerializeExtraData());
            }
        }
    }
}