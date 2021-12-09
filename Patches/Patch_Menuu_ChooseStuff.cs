using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("ChooseStuff")]
    static class Patch_Menuu_ChooseStuff
	{
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, int a, ref int ___stuffSelecting)
        {
			if (PatchMethods.characterFeatureRegistries.TryGetValue(___stuffSelecting, out ICharacterFeatureRegistry characterFeatureRegistry))
            {
                int featureIndex = (characterFeatureRegistry.GetCurrentPage() - 1) * characterFeatureRegistry.GetPageSize() + a;
                if (characterFeatureRegistry.IsFeatureUnlocked(featureIndex))
                {
                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/CLICK1"), Menuu.soundLevel / 10f);
                    characterFeatureRegistry.SelectFeature(featureIndex);
                    InstanceTracker.Menuu.stuffChosen.SetActive((characterFeatureRegistry.GetSelectedFeature() / characterFeatureRegistry.GetPageSize()) + 1 == characterFeatureRegistry.GetCurrentPage());
                    __instance.stuffChosen.transform.position = new Vector3(__instance.box[a].transform.position.x, __instance.box[a].transform.position.y, -3f);
                    __instance.RefreshPlayer();
                }
                return false;
            }
			else
            {
				return true;
            }
        }
    }
}