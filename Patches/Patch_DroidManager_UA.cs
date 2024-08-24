using HarmonyLib;
using GadgetCore.API;
using System.Reflection.Emit;
using System.Collections.Generic;
using GadgetCore.Util;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(DroidManager))]
    [HarmonyPatch("UA")]
    internal static class Patch_DroidManager_UA
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var p = TranspilerHelper.CreateProcessor(instructions, generator);
            var conditionalRef = p.FindRefByInsns(new[] {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldc_I4, 1000),
                new CodeInstruction(OpCodes.Blt),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldc_I4, 2000),
                new CodeInstruction(OpCodes.Bge)
            });
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            p.InjectHook(conditionalRef, typeof(Registry<ItemRegistry, ItemInfo, ItemType>).GetProperty("Singleton").GetGetMethod());
            p.InjectInsns(conditionalRef, new[] {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelem_I4)
            });
            p.InjectHook(conditionalRef, typeof(Registry<ItemRegistry, ItemInfo, ItemType>).GetMethod("HasEntry", new[] { typeof(int) }));
            p.InjectInsn(conditionalRef, new CodeInstruction(OpCodes.Brfalse, l1));
            p.InjectHook(conditionalRef, typeof(Registry<ItemRegistry, ItemInfo, ItemType>).GetProperty("Singleton").GetGetMethod());
            p.InjectInsns(conditionalRef, new[] {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelem_I4)
            });
            p.InjectHook(conditionalRef, typeof(Registry<ItemRegistry, ItemInfo, ItemType>).GetMethod("GetEntry", new[] { typeof(int) }));
            p.InjectInsns(conditionalRef, new[] {
                new CodeInstruction(OpCodes.Ldfld, typeof(ItemInfo).GetField("Type")),
                new CodeInstruction(OpCodes.Ldc_I4, (int)(ItemType.EQUIP_MASK | ItemType.TYPE_MASK)),
                new CodeInstruction(OpCodes.And),
                new CodeInstruction(OpCodes.Ldc_I4, (int)ItemType.DROID),
                new CodeInstruction(OpCodes.Beq, l2)
            });
            p.GetInsn(conditionalRef).labels.Add(l1);
            p.GetInsn(conditionalRef.GetRefByOffset(10)).labels.Add(l2);
            return p.GetInstructions();
        }
    }
}