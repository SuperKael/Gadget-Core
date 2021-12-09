using HarmonyLib;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("RefreshStuffSelect")]
    static class Patch_Menuu_RefreshStuffSelect
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, int a, ref int ___stuffSelecting)
        {
			if (PatchMethods.characterFeatureRegistries.TryGetValue(___stuffSelecting, out ICharacterFeatureRegistry characterFeatureRegistry))
			{
				int featureIndex = (characterFeatureRegistry.GetCurrentPage() - 1) * characterFeatureRegistry.GetPageSize() + a;
				if (characterFeatureRegistry.TryGetEntryInterface(featureIndex, out ICharacterFeatureRegistryEntry entry))
                {
					if (characterFeatureRegistry.IsFeatureUnlocked(featureIndex))
					{
						__instance.stuffName[0].text = entry.GetName();
						__instance.stuffName[1].text = __instance.stuffName[0].text;
						__instance.stuffDesc[0].text = entry.GetDesc();
						__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
						__instance.stuffUnlock[0].text = entry.GetUnlockCondition();
						__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
						__instance.RefreshRaceStats(___stuffSelecting == 0 ? featureIndex : -1);
					}
					else
					{
						__instance.stuffName[0].text = entry.GetLockedName();
						__instance.stuffName[1].text = __instance.stuffName[0].text;
						__instance.stuffDesc[0].text = entry.GetLockedDesc();
						__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
						__instance.stuffUnlock[0].text = entry.GetLockedUnlockCondition();
						__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
						__instance.RefreshRaceStats(-1);
					}
				}
				else if (characterFeatureRegistry.GetCurrentPage() == 1)
				{
					switch (___stuffSelecting)
                    {
						case 0:
							if (characterFeatureRegistry.IsFeatureUnlocked(featureIndex))
							{
								__instance.stuffName[0].text = __instance.GetRaceName(featureIndex);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetRaceDesc(featureIndex);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetRaceUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
								__instance.RefreshRaceStats(featureIndex);
							}
							else
							{
								__instance.stuffName[0].text = __instance.GetRaceName(-1);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetRaceDesc(-1);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetRaceUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
								__instance.RefreshRaceStats(-1);
							}
							break;
						case 1:
							if (characterFeatureRegistry.IsFeatureUnlocked(featureIndex))
							{
								__instance.stuffName[0].text = __instance.GetAugmentName(featureIndex);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetAugmentDesc(featureIndex);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetAugmentUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
							}
							else
							{
								__instance.stuffName[0].text = __instance.GetAugmentName(-1);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetAugmentDesc(-1);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetAugmentUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
							}
							__instance.RefreshRaceStats(-1);
							break;
						case 2:
							if (characterFeatureRegistry.IsFeatureUnlocked(featureIndex))
							{
								__instance.stuffName[0].text = __instance.GetUniformName(featureIndex);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetUniformDesc(featureIndex);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetUniformUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
							}
							else
							{
								__instance.stuffName[0].text = __instance.GetUniformName(-1);
								__instance.stuffName[1].text = __instance.stuffName[0].text;
								__instance.stuffDesc[0].text = __instance.GetUniformDesc(-1);
								__instance.stuffDesc[1].text = __instance.stuffDesc[0].text;
								__instance.stuffUnlock[0].text = __instance.GetUniformUnlock(featureIndex);
								__instance.stuffUnlock[1].text = __instance.stuffUnlock[0].text;
							}
							__instance.RefreshRaceStats(-1);
							break;
					}
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