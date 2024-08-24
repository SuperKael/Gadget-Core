using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("RefreshRaceStats")]
    internal static class Patch_Menuu_RefreshRaceStats
	{
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            var p = TranspilerHelper.CreateProcessor(instructions, gen);
            var ilRef = p.FindRefByInsn(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)99));
            p.InjectInsn(ilRef, new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)-1), false);
            return p.Insns;
        }
    }
}