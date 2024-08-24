using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshChipStats")]
    internal static class Patch_GameScript_RefreshChipStats
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int[] ___combatChips, int[] ___CHIPSTAT)
        {
            for (int i = 0; i < 6; i++)
            {
                int chipID = ___combatChips[i];
                if (ChipRegistry.Singleton.HasEntry(chipID))
                {
                    switch (chipID)
                    {
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            ___CHIPSTAT[chipID - 2] -= 3;
                            break;
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                            ___CHIPSTAT[chipID - 52] -= 7;
                            break;
                        case 102:
                        case 103:
                        case 104:
                        case 105:
                        case 106:
                        case 107:
                            ___CHIPSTAT[chipID - 102] -= 15;
                            break;
                    }
                    ChipRegistry.Singleton.GetEntry(chipID).Stats.AddTo(___CHIPSTAT);
                }
            }
        }
    }
}