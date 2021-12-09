using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("SelectVariant")]
    static class Patch_Menuu_SelectVariant
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance)
        {
            if (CharacterRaceRegistry.Singleton.TryGetEntry(Menuu.curRace, out CharacterRaceInfo entry))
            {
				if (Menuu.curVariant < Math.Min(entry.GetFeatureUnlockLevel(), entry.GetVariantCount()) - 1)
				{
					Menuu.curVariant++;
				}
				else
				{
					Menuu.curVariant = 0;
				}
				__instance.txtVariant[0].text = "Variant: " + (Menuu.curVariant + 1);
				__instance.txtVariant[1].text = __instance.txtVariant[0].text;
				__instance.playerHead.GetComponent<Renderer>().material = (Material)Resources.Load(string.Concat(new object[]
				{
					"r/r",
					Menuu.curRace,
					"v",
					Menuu.curVariant
				}));
				return false;
            }
            return true;
        }
    }
}