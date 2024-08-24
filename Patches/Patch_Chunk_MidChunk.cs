using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using GadgetCore.Util;
using System.Reflection.Emit;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("MidChunk")]
    internal static class Patch_Chunk_MidChunk
    {
        public static readonly MethodInfo PlanetIsTownOnly = typeof(PatchMethods).GetMethod("PlanetIsTownOnly", BindingFlags.Public | BindingFlags.Static);

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            var p = TranspilerHelper.CreateProcessor(instructions, gen);
            var ilRef = p.FindRefByInsns(new[] {
                new CodeInstruction(OpCodes.Ldc_I4_8),
                new CodeInstruction(OpCodes.Beq),
                new CodeInstruction(OpCodes.Ldsfld),
                new CodeInstruction(OpCodes.Ldc_I4_S, (byte)11),
                new CodeInstruction(OpCodes.Beq)
            });
            var brRef = ilRef.GetRefByOffset(4);
            p.RemoveInsns(ilRef, 4);
            p.InjectHook(brRef, PlanetIsTownOnly);
            p.InjectInsn(brRef, new CodeInstruction(OpCodes.Brtrue, p.GetInsn(brRef).operand), false);
            return p.Insns;
        }
    }
}