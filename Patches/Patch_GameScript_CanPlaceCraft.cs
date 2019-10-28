using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("CanPlaceCraft")]
    static class Patch_GameScript_CanPlaceCraft
    {
        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref Item[] ___craft, ref int ___craftType, int a, ref bool __result)
        {
            if (___craftType == 3)
            {
                __result = (bool)typeof(GameScript).GetMethod("CanPlaceCraft2", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { a });
                return false;
            }
            if (ItemRegistry.GetSingleton().HasEntry(a))
            {
                ItemInfo item = ItemRegistry.GetSingleton().GetEntry(a);
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
                        if ((ItemRegistry.GetSingleton().HasEntry(___craft[i].id) ? (int)(ItemRegistry.GetSingleton().GetEntry(a).Type & ItemType.TIER_MASK) >> 8 : ___craft[i].id % 10) != (int)(item.Type & ItemType.TIER_MASK) >> 8)
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