using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Linq;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class Patch_GameScript_LevelUp2
    {
        public static Type IteratorType = typeof(GameScript).GetNestedType("<LevelUp2>c__IteratorD", BindingFlags.NonPublic);
        public static FieldInfo This = IteratorType.GetField("$this", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo PC = IteratorType.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return IteratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyPrefix]
        public static void Prefix(IEnumerator __instance, ref int[] __state)
        {
            if (PC.GetValue(__instance) as int? == 0)
            {
                __state = GameScript.playerBaseStat.ToArray();
            }
        }

        [HarmonyPostfix]
        public static void Postfix(IEnumerator __instance, bool __result, ref int[] __state)
        {
            if (!__result)
            {
                PatchMethods.InvokeOnLevelUp(GameScript.playerLevel);
                GameScript thisInstance = This.GetValue(__instance) as GameScript;
                for (int i = 0; i < __state.Length; i++)
                {
                    thisInstance.statUptxt[i].text = thisInstance.GetStatN(i) + "+" + (GameScript.playerBaseStat[i] - __state[i]);
                    thisInstance.statUptxt[i + __state.Length].text = thisInstance.statUptxt[i].text;
                    thisInstance.statUp[i].SendMessage("Play");
                }
                thisInstance.UpdateHP();
                thisInstance.UpdateMana();
                thisInstance.UpdateEnergy();
                thisInstance.RefreshStats();
            }
        }
    }
}