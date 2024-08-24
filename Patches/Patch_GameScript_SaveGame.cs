using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SaveGame")]
    internal static class Patch_GameScript_SaveGame
    {
        [HarmonyPostfix]
        public static void Postfix(ref Item[] ___inventory, ref Item[] ___gatherStorage, ref Item[] ___storage)
        {
            for (int i = 0; i < ___inventory.Length; i++)
            {
                if (___inventory[i] != null) PreviewLabs.PlayerPrefs.SetString(string.Concat(Menuu.curChar, i, "extraData"), ___inventory[i].SerializeExtraData());
            }
            for (int i = 0; i < ___gatherStorage.Length; i++)
            {
                if (___gatherStorage[i] != null) PreviewLabs.PlayerPrefs.SetString(string.Concat("gather", i, "extraData"), ___gatherStorage[i].SerializeExtraData());
            }
            for (int i = 0; i < ___storage.Length; i++)
            {
                if (___storage[i] != null) PreviewLabs.PlayerPrefs.SetString(string.Concat("storage", i, "extraData"), ___storage[i].SerializeExtraData());
            }
        }
    }
}