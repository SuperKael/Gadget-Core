using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SwapCombatChip")]
    static class Patch_GameScript_SwapCombatChip
    {
        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, int slot, int[] ___combatChips)
        {
            int chipID = ___combatChips[slot];
            if (ChipRegistry.Singleton.HasEntry(chipID))
            {
                ChipRegistry.Singleton.GetEntry(chipID).InvokeOnDequip(slot);
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int slot, int[] ___combatChips)
        {
            int chipID = ___combatChips[slot];
            if (ChipRegistry.Singleton.HasEntry(chipID))
            {
                ChipRegistry.Singleton.GetEntry(chipID).InvokeOnEquip(slot);
            }
        }
    }
}