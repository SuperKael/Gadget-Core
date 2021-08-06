using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("EraseCharacter")]
    static class Patch_GameScript_EraseCharacter
    {
        [HarmonyPostfix]
        public static void Postfix(int a, ref Item[] ___inventory)
        {
            for (int i = 0; i < ___inventory.Length; i++)
            {
                PreviewLabs.PlayerPrefs.SetString(string.Concat(a, i, "extraData"), string.Empty);
            }
        }
    }
}