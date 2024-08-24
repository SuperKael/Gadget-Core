using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("PlaceCombatChip")]
    internal static class Patch_GameScript_PlaceCombatChip
    {
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