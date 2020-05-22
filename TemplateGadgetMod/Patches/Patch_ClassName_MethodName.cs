using HarmonyLib;
using GadgetCore.API;
using UnityEngine;

namespace TemplateGadgetMod.Patches // You may create multiple files like this. Use the prefix, the postfix, or both. You can also use a Transpiler if you wish.
{
    [HarmonyPatch(typeof(ClassName))]
    [HarmonyPatch("MethodName")]          // TODO: Correct this information
    [HarmonyGadget("TemplateGadgetMod")]
    public static class Patch_ClassName_MethodName
    {
		/*
		[HarmonyPrefix]
        public static bool Prefix(ClassName __instance)
        {
            // Add code to run before `MethodName` is called.
			return true; // Return false to prevent the vanilla method from running.
        }
		*/
		
		/*
        [HarmonyPostfix]
        public static void Postfix(ClassName __instance)
        {
            // Add code to run after `MethodName` is called.
        }
		*/
    }
}
