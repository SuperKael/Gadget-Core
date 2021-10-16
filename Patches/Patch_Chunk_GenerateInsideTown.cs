using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("GenerateInsideTown")]
    static class Patch_Chunk_GenerateInsideTown
    {
        public static readonly FieldInfo networkStuffField = typeof(Chunk).GetField("networkStuff", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo tempField = typeof(Chunk).GetField("temp", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPostfix]
        public static void Postfix(Chunk __instance, ref IEnumerator __result)
        {
            if (PlanetRegistry.Singleton[SpawnerScript.curBiome] is PlanetInfo planet)
            {
                __result = GenerateWrapper(__result, __instance, planet);
            }
        }

        private static IEnumerator GenerateWrapper(IEnumerator generateRoutine, Chunk instance, PlanetInfo planet)
        {
            yield return instance.StartCoroutine(generateRoutine);
            IEnumerable<GameObject> objs = planet.InvokeOnGenerateInsideTown(instance);
            if (objs == null) yield break;
            GameObject[] networkStuff = networkStuffField.GetValue<GameObject[]>(instance);
            int temp = tempField.GetValue<int>(instance);
            foreach (GameObject obj in objs)
            {
                if (obj == null) continue;
                networkStuff[temp] = obj;
                temp++;
            }
            tempField.SetValue(instance, temp);
            yield break;
        }
    }

    [HarmonyPatch]
    static class Patch_Chunk_GenerateInsideTown_MoveNext
    {
        public static readonly MethodInfo PlanetIsTownOnly = typeof(PatchMethods).GetMethod("PlanetIsTownOnly", BindingFlags.Public | BindingFlags.Static);
        public static readonly FieldInfo curBiome = typeof(SpawnerScript).GetField("curBiome", BindingFlags.Public | BindingFlags.Static);

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(Chunk).GetNestedType("<GenerateInsideTown>c__Iterator1", BindingFlags.NonPublic).GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            var p = TranspilerHelper.CreateProcessor(instructions, gen);
            var ilRef = p.FindRefByInsns(new CodeInstruction[] {
                new CodeInstruction(OpCodes.Br),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldstr, "obj/gearChalice")
            });
            p.InjectLoadField(ilRef + 1, curBiome);
            p.InjectHook(ilRef + 2, PlanetIsTownOnly);
            p.InjectInsn(ilRef + 3, new CodeInstruction(OpCodes.Brtrue, p.GetInsn(ilRef).operand));
            return p.Insns;
        }
    }
}