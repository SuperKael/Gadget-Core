using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("HoverCombatChip")]
    internal static class Patch_GameScript_HoverCombatChip
    {
        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int slot, int[] ___combatChips)
        {
            int chipID = ___combatChips[slot];
            if (ChipRegistry.Singleton.HasEntry(chipID))
            {
                ChipInfo chip = ChipRegistry.Singleton.GetEntry(chipID);
                if ((chip.Type & ChipType.ACTIVE) > 0 && chip.Cost > 0)
                {
                    __instance.txtChipCost[0].text = chip.Cost + (chip.CostType == ChipInfo.ChipCostType.MANA ? " Mana" : (chip.CostType == ChipInfo.ChipCostType.ENERGY ? " Energy" : " Health"));
                    __instance.txtChipCost[1].text = __instance.txtChipCost[0].text;
                }
            }
            else
            {
                string chipName = GadgetCoreAPI.GetChipName(chipID);
                if (chipName == "Missing Modded Combat Chip!" || chipName == "Invalid Combat Chip!")
                {
                    __instance.txtChipCost[0].text = string.Empty;
                    __instance.txtChipCost[1].text = string.Empty;
                }
            }
        }
    }
}