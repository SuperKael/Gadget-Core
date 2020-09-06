using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using GadgetCore.Util;
using System.IO;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Update")]
    static class Patch_GameScript_Update
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, ref Item[] ___inventory, ref int ___curBlockSlot, ref ChunkWorld ___chunkWorld, ref ChunkWorld ___chunkWorldWall)
        {
            if (!GameScript.pausing && !GameScript.inventoryOpen && GameScript.buildMode && Input.GetMouseButtonDown(0) && ___inventory[___curBlockSlot].q > 0 && ItemRegistry.GetSingleton().HasEntry(___inventory[___curBlockSlot].id))
            {
                int num4 = (int)__instance.hoverBuild.transform.position.x;
                int num5 = (int)__instance.hoverBuild.transform.position.y;
                int num6 = (num4 + 62) / 4;
                int num7 = (num5 + 62) / 4;
                if (num6 < 64 && num6 >= 0 && num7 < 64 && num7 >= 0)
                {
                    ItemInfo item = ItemRegistry.GetSingleton().GetEntry(___inventory[___curBlockSlot].id);
                    if (item.Tile != null && item.Tile.TryPlaceTile())
                    {
                        if (item.Tile.Type == TileType.WALL)
                        {
                            if (___chunkWorldWall.grid[num6, num7] == 0)
                            {
                                int[] value = new int[]
                                {
                                        num4,
                                        num5,
                                        GameScript.curBlockID
                                };
                                __instance.WallManager.SendMessage("PlaceTile", value);
                                __instance.hoverBuild.SendMessage("Refresh");
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                ___inventory[___curBlockSlot].q--;
                                RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                {
                                    __instance.ExitBuildMode();
                                    __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                    GameScript.curBlockID = 0;
                                }
                            }
                        }
                        else if (item.Tile.Type == TileType.SOLID)
                        {
                            if (___chunkWorld.gridSpecial[num6, num7] == 0 && ___chunkWorld.grid[num6, num7] == 0)
                            {
                                int[] value3 = new int[]
                                {
                                    num4,
                                    num5,
                                    GameScript.curBlockID
                                };
                                __instance.TileManager.SendMessage("PlaceTile", value3);
                                __instance.hoverBuild.SendMessage("Refresh");
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                ___inventory[___curBlockSlot].q--;
                                RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                {
                                    __instance.ExitBuildMode();
                                    __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                    GameScript.curBlockID = 0;
                                }
                            }
                        }
                        else
                        {
                            if (___chunkWorld.gridSpecial[num6, num7] == 0 && ___chunkWorld.grid[num6, num7] == 0)
                            {
                                if (GameScript.curBlockID == 2107 && __instance.gatherer)
                                {
                                    __instance.Error(11);
                                }
                                else
                                {
                                    int[] value2 = new int[]
                                    {
                                            num4,
                                            num5,
                                            GameScript.curBlockID
                                    };
                                    __instance.TileManager.SendMessage("PlaceTileSpecial", value2);
                                    __instance.hoverBuild.SendMessage("Refresh");
                                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                    ___inventory[___curBlockSlot].q--;
                                    RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                    if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                    {
                                        __instance.ExitBuildMode();
                                        __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                        GameScript.curBlockID = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            GadgetCore.CoreLogger.Log(OpCodes.Call.StackBehaviourPop);
            CodeInstruction[] codes = instructions.ToArray();
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            for (int i = 0; i < 11;i++) newCodes.Add(codes[i]);
            for (int i = 11; i < codes.Length; i++)
            {   
                newCodes.Add(codes[i]);
                if ((codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Ldsfld && codes[i - 1].operand.ToString().Equals("System.Boolean pausing")
                 && codes[i] != null && codes[i].opcode == OpCodes.Brtrue) ||
                    (codes[i - 2] != null && codes[i - 2].opcode == OpCodes.Ldc_I4_S && ((sbyte)codes[i - 2].operand) == 98
                 && codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Call && (codes[i - 1].operand as MethodBase).Name == "GetKeyDown"
                 && codes[i] != null && codes[i].opcode == OpCodes.Brfalse))
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Call, typeof(GadgetCoreAPI).GetMethod("IsInputFrozen", BindingFlags.Public | BindingFlags.Static)));
                    newCodes.Add(new CodeInstruction(OpCodes.Brtrue, codes[i].operand));
                }
                else if (codes[i - 11] != null && codes[i - 11].opcode == OpCodes.Ldloc_S
                 && codes[i - 10] != null && codes[i - 10].opcode == OpCodes.Ldc_I4_S
                 && codes[i - 9] != null && codes[i - 9].opcode == OpCodes.Bge
                 && codes[i - 8] != null && codes[i - 8].opcode == OpCodes.Ldloc_S
                 && codes[i - 7] != null && codes[i - 7].opcode == OpCodes.Ldc_I4_0
                 && codes[i - 6] != null && codes[i - 6].opcode == OpCodes.Blt
                 && codes[i - 5] != null && codes[i - 5].opcode == OpCodes.Ldloc_S
                 && codes[i - 4] != null && codes[i - 4].opcode == OpCodes.Ldc_I4_S
                 && codes[i - 3] != null && codes[i - 3].opcode == OpCodes.Bge
                 && codes[i - 2] != null && codes[i - 2].opcode == OpCodes.Ldloc_S
                 && codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Ldc_I4_0
                 && codes[i] != null && codes[i].opcode == OpCodes.Blt)
                {
                    if (codes.Length > i + 1 && codes[i + 1].opcode == OpCodes.Ldsfld)
                    {
                        newCodes.Add(new CodeInstruction(OpCodes.Ldsfld, codes[i + 1].operand));
                        newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4, TileRegistry.GetSingleton().GetIDStart()));
                        newCodes.Add(new CodeInstruction(OpCodes.Bge, codes[i].operand));
                    }
                }
            }
            var p = TranspilerHelper.CreateProcessor(newCodes, generator);
            var forgeEmblemBlock = p.FindRefByInsn(new CodeInstruction(OpCodes.Call, "Void ForgeEmblem(Int32)"));
            p.InjectInsn(forgeEmblemBlock.GetRefByOffset(-21), new CodeInstruction(OpCodes.Brtrue, p.GetInsn(forgeEmblemBlock.GetRefByOffset(-20)).operand), false);
            p.RemoveInsns(forgeEmblemBlock.GetRefByOffset(-20), 17);
            p.RemoveInsns(forgeEmblemBlock.GetRefByOffset(1), 6);
            return p.GetInstructions();
        }
    }
}