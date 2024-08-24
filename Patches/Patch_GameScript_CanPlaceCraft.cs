using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CanPlaceCraft")]
    internal static class Patch_GameScript_CanPlaceCraft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item[] ___craft, ref int ___craftType, int a, ref bool __result)
        {
            if (MenuRegistry.Singleton[___craftType] is CraftMenuInfo)
            {
                __result = !Input.GetMouseButtonDown(1);
                return false;
            }
            if (___craftType == 3)
            {
                __result = __instance.CanPlaceCraft2(a);
                return false;
            }
            if (ItemRegistry.Singleton.HasEntry(a))
            {
                ItemInfo item = ItemRegistry.Singleton.GetEntry(a);
                __result = true;
                for (int i = 0; i < 3; i++)
                {
                    if (___craft[i].id == a)
                    {
                        __result = false;
                        __instance.Error(4);
                        break;
                    }
                    if (___craft[i].id > 0)
                    {
                        if ((ItemRegistry.Singleton.HasEntry(___craft[i].id) ? (int)(ItemRegistry.Singleton.GetEntry(a).Type & ItemType.TIER_MASK) >> 8 : ___craft[i].id % 10) != (int)(item.Type & ItemType.TIER_MASK) >> 8)
                        {
                            __result = false;
                            __instance.Error(5);
                            break;
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}