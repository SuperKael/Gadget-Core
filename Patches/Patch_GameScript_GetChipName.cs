using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetChipName")]
    internal static class Patch_GameScript_GetChipName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ChipRegistry.Singleton.HasEntry(id))
            {
                __result = ChipRegistry.Singleton.GetEntry(id).Name;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(int id, ref string __result)
        {
            if (id != 0 && string.IsNullOrEmpty(__result))
            {
                if (id >= ChipRegistry.Singleton.GetIDStart())
                {
                    if (ChipRegistry.Singleton.GetEntry(id) == null)
                    {
                        __result = "Missing Modded Combat Chip!";
                    }
                }
                else
                {
                    __result = "Invalid Combat Chip!";
                }
            }
        }
    }
}