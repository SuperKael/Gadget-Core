using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("EraseCharacter")]
    internal static class Patch_Menuu_EraseCharacter
    {
        [HarmonyPrefix]
        public static void Prefix(int a)
        {
            for (int i = 0; i < 45; i++)
            {
                PreviewLabs.PlayerPrefs.SetString(string.Concat(a, i, "extraData"), string.Empty);
            }
        }
    }
}