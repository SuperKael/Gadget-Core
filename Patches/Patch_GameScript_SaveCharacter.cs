using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SaveCharacter")]
    internal static class Patch_GameScript_SaveCharacter
    {
        [HarmonyPostfix]
        public static void Postfix(ref Item[] ___inventory)
        {
            for (int i = 0; i < ___inventory.Length; i++)
            {
                if (___inventory[i] != null) PreviewLabs.PlayerPrefs.SetString(string.Concat(Menuu.curChar, i, "extraData"), ___inventory[i].SerializeExtraData());
            }
        }
    }
}