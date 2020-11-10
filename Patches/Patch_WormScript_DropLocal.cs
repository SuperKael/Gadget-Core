using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(WormScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_WormScript_DropLocal
    {
        public static readonly MethodInfo SpawnExp = typeof(GadgetCoreAPI).GetMethod("SpawnExp", BindingFlags.Public | BindingFlags.Static);
        public static readonly MethodInfo get_position = typeof(Transform).GetMethod("get_position", BindingFlags.Public | BindingFlags.Instance);
        public static readonly FieldInfo t = typeof(WormScript).GetField("t", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPostfix]
        public static void Postfix(WormScript __instance)
        {
            LootTables.DropLoot("entity:" + __instance.name.Split(' ', '(')[0], __instance.transform.position);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            TranspilerHelper.CILProcessor p = TranspilerHelper.CreateProcessor(instructions, gen);

            TranspilerHelper.CILProcessor.ILRef recordRef = p.FindRefByInsns(new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32[] record"),
                new CodeInstruction(OpCodes.Ldsfld, "System.Int32 curBiome"),
                new CodeInstruction(OpCodes.Ldelema, "System.Int32"),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldind_I4),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stind_I4)
                });

            Label label = gen.DefineLabel();
            p.GetInsn(recordRef.GetRefByOffset(8)).labels.Add(label);
            p.InjectInsn(recordRef, p.GetInsn(recordRef.GetRefByOffset(1)));
            p.InjectInsn(recordRef, new CodeInstruction(OpCodes.Ldc_I4_8));
            p.InjectInsn(recordRef, new CodeInstruction(OpCodes.Bge, label));

            TranspilerHelper.CILProcessor.ILRef loopRef = p.FindRefByInsns(new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, "System.Int32 exp"),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Bgt)
                });
            TranspilerHelper.CILProcessor.ILRef branchRef = p.FindRefByInsn(new CodeInstruction(OpCodes.Br, p.GetInsn(loopRef).labels[0]));
            List<Label> labels = p.GetInsn(branchRef).labels;
            p.RemoveInsns(branchRef, loopRef.Index - branchRef.Index);
            TranspilerHelper.CILProcessor.ILRef newRef = p.InjectInsns(loopRef, new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, t),
                new CodeInstruction(OpCodes.Callvirt, get_position)
                });
            p.InjectInsns(loopRef.GetRefByOffset(2), new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldc_R4, 0f),
                new CodeInstruction(OpCodes.Call, SpawnExp)
                }, false);
            p.GetInsn(newRef).labels.AddRange(labels);
            return p.Insns;
        }
    }
}