using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("GetItemName")]
    static class Patch_GameScript_GetItemName
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, int id, ref string __result)
        {
            if (ItemRegistry.Singleton.HasEntry(id))
            {
                __result = ItemRegistry.Singleton.GetEntry(id).Name;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(int id, ref string __result)
        {
            if (id != 0 && string.IsNullOrEmpty(__result))
            {
                if (id >= ItemRegistry.Singleton.GetIDStart())
                {
                    if (ItemRegistry.Singleton.GetEntry(id) == null)
                    {
                        __result = "Missing Mod Item!";
                    }
                }
                else
                {
                    __result = "Invalid Item!";
                }
            }
        }
    }
}