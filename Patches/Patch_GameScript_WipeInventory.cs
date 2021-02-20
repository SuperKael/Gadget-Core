using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("WipeInventory")]
    static class Patch_GameScript_WipeInventory
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, ref Item[] ___inventory)
        {
            for (int i = 0; i < 45; i++)
            {
                ___inventory[i].RemoveAllExtraData();
                PreviewLabs.PlayerPrefs.SetString(Menuu.curChar + i + "ed", ___inventory[i].SerializeExtraData());
            }
        }
    }
}