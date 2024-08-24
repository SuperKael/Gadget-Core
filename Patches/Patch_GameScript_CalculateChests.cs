using HarmonyLib;
using GadgetCore.API;
using System;
using System.Reflection;
using System.Collections;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    internal static class Patch_GameScript_CalculateChests
    {
        public static Type IteratorType = typeof(GameScript).GetNestedType("<CalculateChests>c__Iterator38", BindingFlags.NonPublic);
        public static FieldInfo This = IteratorType.GetField("$this", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo PC = IteratorType.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return IteratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyPostfix]
        public static void Postfix(IEnumerator __instance, bool __result)
        {
            if ((int)PC.GetValue(__instance) == 1)
            {
                GameScript gameScript = (GameScript)This.GetValue(__instance);
                foreach (ICharacterFeatureRegistryEntry feature in PatchMethods.characterFeatureRegistryEntries.Values)
                {
                    if (feature.ShouldUnlock())
                    {
                        gameScript.AddChest(feature.GetChestID());
                    }
                }
                gameScript.RefreshChests();
            }
        }
    }
}