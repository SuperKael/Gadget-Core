using HarmonyLib;
using GadgetCore.API;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Update")]
    static class Patch_PlayerScript_Update
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] codes = instructions.ToArray();
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            for (int i = 0; i < 11; i++) newCodes.Add(codes[i]);
            for (int i = 11; i < codes.Length; i++)
            {
                newCodes.Add(codes[i]);
                if (codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Ldsfld && codes[i - 1].operand.ToString().Equals("System.Boolean pausing")
                 && codes[i] != null && codes[i].opcode == OpCodes.Brtrue)
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Call, typeof(GadgetCoreAPI).GetMethod("IsInputFrozen", BindingFlags.Public | BindingFlags.Static)));
                    newCodes.Add(new CodeInstruction(OpCodes.Brtrue, codes[i].operand));
                }
            }
            return newCodes;
        }
    }
}