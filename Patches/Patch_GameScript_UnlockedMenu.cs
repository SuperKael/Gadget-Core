using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class Patch_GameScript_UnlockedMenu
    {
        public static Type IteratorType = typeof(GameScript).GetNestedType("<UnlockedMenu>c__Iterator15", BindingFlags.NonPublic);
        public static FieldInfo This = IteratorType.GetField("$this", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo PC = IteratorType.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return IteratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyPostfix]
        public static void Postfix(IEnumerator __instance, bool __result, int ___id)
        {
            if ((int)PC.GetValue(__instance) == 1)
            {
                GameScript gameScript = (GameScript)This.GetValue(__instance);
                gameScript.unlockedIcon.transform.parent.localScale = Vector3.one;
                if (PatchMethods.characterFeatureRegistryEntries.TryGetValue(___id, out ICharacterFeatureRegistryEntry feature))
                {
                    feature.OpenChest();
                }
            }
        }
    }
}