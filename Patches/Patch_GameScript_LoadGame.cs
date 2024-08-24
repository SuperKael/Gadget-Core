using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("LoadGame")]
    internal static class Patch_GameScript_LoadGame
    {
        [HarmonyPostfix]
        public static void Postfix(ref Item[] ___inventory, ref Item[] ___gatherStorage, ref Item[] ___storage)
        {
            for (int i = 0; i < ___inventory.Length; i++)
            {
                ___inventory[i]?.DeserializeExtraData(PreviewLabs.PlayerPrefs.GetString(string.Concat(Menuu.curChar, i, "extraData")));
            }
            for (int i = 0; i < ___gatherStorage.Length; i++)
            {
                ___gatherStorage[i]?.DeserializeExtraData(PreviewLabs.PlayerPrefs.GetString(string.Concat("gather", i, "extraData")));
            }
            for (int i = 0; i < ___storage.Length; i++)
            {
                ___storage[i]?.DeserializeExtraData(PreviewLabs.PlayerPrefs.GetString(string.Concat("storage", i, "extraData")));
            }
        }
    }
}