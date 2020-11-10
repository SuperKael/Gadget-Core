using HarmonyLib;
using GadgetCore.API;

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
                if (ChipRegistry.Singleton.HasEntry(chipID))
                {
                    ChipRegistry.Singleton.GetEntry(chipID).InvokeOnDequip(slot);
                }
            }
        }
    }
}