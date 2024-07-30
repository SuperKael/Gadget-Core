using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetChipDesc")]
    static class Patch_GameScript_GetChipDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ChipRegistry.Singleton.HasEntry(id))
            {
                __result = ChipRegistry.Singleton.GetEntry(id).Desc;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int id, ref string __result)
        {
            if (id != 0 && string.IsNullOrEmpty(__result) && (__instance.GetChipName(id) == "Missing Modded Combat Chip!" || __instance.GetChipName(id) == "Invalid Combat Chip!"))
            {
                if (id >= ChipRegistry.Singleton.GetIDStart())
                {
                    if (ChipRegistry.Singleton.GetEntry(id) == null)
                    {
                        string modID = ChipRegistry.Singleton.IsIDReserved(id);
                        if (modID != null)
                        {
                            __result = "Unloaded Combat Chip from the mod:\n" + modID;
                        }
                        else
                        {
                            __result = "Invalid Combat Chip from an unknown mod.";
                        }
                    }
                }
                else
                {
                    string itemName = GadgetCoreAPI.GetItemName(id);
                    if (itemName != "Missing Mod Item!" && itemName != "Invalid Item!")
                    {
                        __result = "An invalid Combat Chip. Shares its ID with the Item:\n" + itemName;
                    }
                    else
                    {
                        __result = "An invalid Combat Chip.";
                    }
                }
            }
        }
    }
}