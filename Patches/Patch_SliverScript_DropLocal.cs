using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using GadgetCore.Util;
using UnityEngine;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(SliverScript))]
    [HarmonyPatch("DropLocal")]
    static class Patch_SliverScript_DropLocal
    {
        public static readonly MethodInfo SpawnExp = typeof(GadgetCoreAPI).GetMethod("SpawnExp", BindingFlags.Public | BindingFlags.Static);
        public static readonly MethodInfo get_position = typeof(Transform).GetMethod("get_position", BindingFlags.Public | BindingFlags.Instance);
        public static readonly FieldInfo t = typeof(SliverScript).GetField("t", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPostfix]
        public static void Postfix(SliverScript __instance)
        {
            LootTables.DropLoot("entity:" + (__instance.wormDisassemble != null ? __instance.wormDisassemble.transform : __instance.transform).GetHighestParent().name.Split('(')[0], __instance.transform.position);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            TranspilerHelper.CILProcessor p = TranspilerHelper.CreateProcessor(instructions, gen);
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