using HarmonyLib;
using GadgetCore.API.Dialog;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SelectChoice")]
    internal static class Patch_GameScript_SelectChoice
    {
        [HarmonyPrefix]
        public static void Prefix(ref int __state)
        {
            __state = GameScript.allegianceQuest;
        }

        public static void Postfix(GameScript __instance, int a, ref int __state)
        {
            GameScript.allegianceQuest = __state + 1;
            DialogChain chain = DialogActions.autoDialogAfterChoice;
            if (chain != null)
            {
                if (DialogActions.autoDialogForce || chain.TriggerCondition == null || chain.TriggerCondition(GameScript.allegianceQuest)) chain.InitiateDialog();
                DialogActions.afterChoiceAction?.Invoke(a + 1);
            }
            DialogActions.autoDialogAfterChoice = null;
            DialogActions.afterChoiceAction = null;
        }
    }
}