using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetItemDesc")]
    internal static class Patch_GameScript_GetItemDesc
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = ItemRegistry.Singleton.GetEntry(id).Desc;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int id, ref string __result)
        {
            if (__instance == null) return;
            if (id != 0 && string.IsNullOrEmpty(__result) && (__instance.GetItemName(id) == "Missing Mod Item!" || __instance.GetItemName(id) == "Invalid Item!"))
            {
                if (id >= ItemRegistry.Singleton.GetIDStart())
                {
                    if (ItemRegistry.Singleton.GetEntry(id) == null)
                    {
                        string modID = ItemRegistry.Singleton.IsIDReserved(id);
                        if (modID != null)
                        {
                            __result = "Unloaded Item\nfrom the mod:\n" + modID;
                        }
                        else
                        {
                            __result = "Invalid Item\nfrom an unknown mod.";
                        }
                    }
                }
                else
                {
                    string chipName = GadgetCoreAPI.GetChipName(id);
                    if (chipName != "Missing Modded Combat Chip!" && chipName != "Invalid Combat Chip!")
                    {
                        __result = "Invalid Item. Shares\nID with Combat Chip:\n" + chipName;
                    }
                    else
                    {
                        __result = "Invalid Item.";
                    }
                }
            }
        }
    }
}