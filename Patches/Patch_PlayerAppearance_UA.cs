using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerAppearance))]
    [HarmonyPatch("UA")]
    static class Patch_PlayerAppearance_UA
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerAppearance __instance, int[] ei, int a, bool dead)
        {
            if (ei[0] >= 10000 && ItemRegistry.GetSingleton().HasEntry(ei[0]) && (ItemRegistry.GetSingleton().GetEntry(ei[0]).GetEntryTypeEnum() & ItemType.WEAPON) == ItemType.WEAPON && GadgetCoreAPI.resources.ContainsKey("ie/ie" + ei[0]))
            {
                __instance.weaponObj.GetComponent<Renderer>().material = (Material)Resources.Load("ie/ie" + ei[0]);
            }
        }
    }
}