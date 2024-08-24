using HarmonyLib;
using System.Collections;
using UnityEngine;
using GadgetCore.API.Dialog;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("MenuStory2")]
    internal static class Patch_GameScript_MenuStory2
    {
		[HarmonyPrefix]
		public static bool Prefix(GameScript __instance, ref IEnumerator __result)
		{
			DialogActions.autoDialogAfterChoice = null;
			DialogActions.afterChoiceAction = null;
			if (DialogActions.storyChoiceOverrideDesc != null)
			{
				__result = MenuStory2(__instance);
				return false;
			}
			else
			{
				__instance.txtChoice0[0].transform.parent.gameObject.SetActive(true);
				__instance.txtChoice1[0].transform.parent.gameObject.SetActive(true);
				__instance.txtChoice2[0].transform.parent.gameObject.SetActive(true);
				return true;
			}
		}

        private static IEnumerator MenuStory2(GameScript __instance)
        {
			yield return new WaitForSeconds(0.3f);
			if (DialogActions.storyChoiceOverrideDesc != null)
			{
				Object.Instantiate(Resources.Load("burst"), MenuScript.player.transform.position, Quaternion.identity);

				__instance.txtStoryDesc[0].text = DialogActions.storyChoiceOverrideDesc;

				if (DialogActions.storyChoiceOverrideChoice1 != null)
				{
					__instance.txtChoice0[0].text = DialogActions.storyChoiceOverrideChoice1;
					__instance.txtChoice0[0].transform.parent.gameObject.SetActive(true);
				}
				else
				{
					__instance.txtChoice0[0].text = string.Empty;
					__instance.txtChoice0[0].transform.parent.gameObject.SetActive(false);
				}
				if (DialogActions.storyChoiceOverrideChoice2 != null)
				{
					__instance.txtChoice1[0].text = DialogActions.storyChoiceOverrideChoice2;
					__instance.txtChoice1[0].transform.parent.gameObject.SetActive(true);
				}
				else
				{
					__instance.txtChoice1[0].text = string.Empty;
					__instance.txtChoice1[0].transform.parent.gameObject.SetActive(false);
				}
				if (DialogActions.storyChoiceOverrideChoice3 != null)
				{
					__instance.txtChoice2[0].text = DialogActions.storyChoiceOverrideChoice3;
					__instance.txtChoice2[0].transform.parent.gameObject.SetActive(true);
				}
				else
				{
					__instance.txtChoice2[0].text = string.Empty;
					__instance.txtChoice2[0].transform.parent.gameObject.SetActive(false);
				}

				__instance.txtStoryDesc[1].text = __instance.txtStoryDesc[0].text;
				__instance.txtChoice0[1].text = __instance.txtChoice0[0].text;
				__instance.txtChoice1[1].text = __instance.txtChoice1[0].text;
				__instance.txtChoice2[1].text = __instance.txtChoice2[0].text;

				__instance.menuStory.SetActive(true);

				DialogActions.storyChoiceOverrideDesc = null;
			}
        }
    }
}