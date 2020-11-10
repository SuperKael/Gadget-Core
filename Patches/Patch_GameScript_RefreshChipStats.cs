using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("RefreshChipStats")]
    static class Patch_GameScript_RefreshChipStats
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
                            ___CHIPSTAT[0] -= 3;
                            break;
                        case 3:
                            ___CHIPSTAT[1] -= 3;
                            break;
                        case 4:
                            ___CHIPSTAT[2] -= 3;
                            break;
                        case 5:
                            ___CHIPSTAT[3] -= 3;
                            break;
                        case 6:
                            ___CHIPSTAT[4] -= 3;
                            break;
                        case 7:
                            ___CHIPSTAT[5] -= 3;
                            break;
                        case 52:
                            ___CHIPSTAT[0] -= 7;
                            break;
                        case 53:
                            ___CHIPSTAT[1] -= 7;
                            break;
                        case 54:
                            ___CHIPSTAT[2] -= 7;
                            break;
                        case 55:
                            ___CHIPSTAT[3] -= 7;
                            break;
                        case 56:
                            ___CHIPSTAT[4] -= 7;
                            break;
                        case 57:
                            ___CHIPSTAT[5] -= 7;
                            break;
                        case 102:
                            ___CHIPSTAT[0] -= 15;
                            break;
                        case 103:
                            ___CHIPSTAT[1] -= 15;
                            break;
                        case 104:
                            ___CHIPSTAT[2] -= 15;
                            break;
                        case 105:
                            ___CHIPSTAT[3] -= 15;
                            break;
                        case 106:
                            ___CHIPSTAT[4] -= 15;
                            break;
                        case 107:
                            ___CHIPSTAT[5] -= 15;
                            break;
                    }
                    ChipRegistry.Singleton.GetEntry(chipID).Stats.AddTo(___CHIPSTAT);
                }
            }
        }
    }
}