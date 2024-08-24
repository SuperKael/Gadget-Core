using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerAppearance))]
    [HarmonyPatch("UA")]
    internal static class Patch_PlayerAppearance_UA
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerAppearance __instance, int[] ei, int a, bool dead)
        {
            if (ei[0] >= ItemRegistry.Singleton.GetIDStart() && ItemRegistry.Singleton.HasEntry(ei[0]) && (ItemRegistry.Singleton.GetEntry(ei[0]).GetEntryType() & ItemType.WEAPON) == ItemType.WEAPON && GadgetCoreAPI.IsCustomResourceRegistered("ie/ie" + ei[0]))
            {
                __instance.weaponObj.GetComponent<Renderer>().material = (Material)Resources.Load("ie/ie" + ei[0]);
            }
        }
    }
}