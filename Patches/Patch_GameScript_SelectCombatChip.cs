using HarmonyLib;
using GadgetCore.API;
using GadgetCore;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SelectCombatChip")]
    static class Patch_GameScript_SelectCombatChip
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int slot, int[] ___combatChips, bool ___exitingcombatmode)
        {
            if (!___exitingcombatmode)
            {
                int chipID = ___combatChips[slot];
                if (ChipRegistry.GetSingleton().HasEntry(chipID))
                {
                    ChipRegistry.GetSingleton().GetEntry(chipID).InvokeOnDequip(slot);
                }
            }
        }
    }
}