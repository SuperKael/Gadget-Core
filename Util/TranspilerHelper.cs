using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Compilation;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace GadgetCore.Util
{
    /// <summary>
    /// Offers utility functions for making Transpilers.
    /// </summary>
    public static class TranspilerHelper
    {
        private static Dictionary<int, OpCode> reversedConditionalCodes;

        static TranspilerHelper()
        {
            reversedConditionalCodes = new Dictionary<int, OpCode>()
            {
                [OpCodes.Brfalse.Value] = OpCodes.Brtrue,
                [OpCodes.Brtrue.Value] = OpCodes.Brfalse,
                [OpCodes.Brfalse_S.Value] = OpCodes.Brtrue_S,
                [OpCodes.Brtrue_S.Value] = OpCodes.Brfalse_S,
                [OpCodes.Beq.Value] = OpCodes.Bne_Un,
                [OpCodes.Bne_Un.Value] = OpCodes.Beq,
                [OpCodes.Beq_S.Value] = OpCodes.Bne_Un_S,
                [OpCodes.Bne_Un_S.Value] = OpCodes.Beq_S,
                [OpCodes.Bge.Value] = OpCodes.Blt,
                [OpCodes.Ble.Value] = OpCodes.Bgt,
                [OpCodes.Blt.Value] = OpCodes.Bge,
                [OpCodes.Bgt.Value] = OpCodes.Ble,
                [OpCodes.Bge_Un.Value] = OpCodes.Blt_Un,
                [OpCodes.Ble_Un.Value] = OpCodes.Bgt_Un,
                [OpCodes.Blt_Un.Value] = OpCodes.Bge_Un,
                [OpCodes.Bgt_Un.Value] = OpCodes.Ble_Un,
                [OpCodes.Bge_S.Value] = OpCodes.Blt_S,
                [OpCodes.Ble_S.Value] = OpCodes.Bgt_S,
                [OpCodes.Blt_S.Value] = OpCodes.Bge_S,
                [OpCodes.Bgt_S.Value] = OpCodes.Ble_S,
                [OpCodes.Bge_Un_S.Value] = OpCodes.Blt_Un_S,
                [OpCodes.Ble_Un_S.Value] = OpCodes.Bgt_Un_S,
                [OpCodes.Blt_Un_S.Value] = OpCodes.Bge_Un_S,
                [OpCodes.Bgt_Un_S.Value] = OpCodes.Ble_Un_S,
            };
        }

        /// <summary>
        /// Dumps the given <see cref="IEnumerable{CodeInstruction}"/> of <see cref="CodeInstruction"/>s as a string,
        /// optionally with a given starting instruction index and count of instructions to print.
        /// lines are seperated by the \n character.
        /// </summary>
        public static string DumpInstructions(IEnumerable<CodeInstruction> insns, int start = 0, int count = int.MaxValue)
        {
            return CreateProcessor(insns, null).DumpInstructions(start, count);
        }

        /// <summary>
        /// Returns the size, in bytes, of the given <see cref="CodeInstruction"/>'s operand.
        /// The size of a complete CodeInstruction is this + <paramref name="insn"/>.opcode.Size.
        /// </summary>
        public static int GetOperandSize(CodeInstruction insn)
        {
            int operandSize = 0;
            switch (insn.opcode.OperandType)
            {
                case OperandType.InlineNone:
                case OperandType.InlinePhi:
                    operandSize = 0;
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    operandSize = 1;
                    break;
                case OperandType.InlineVar:
                    operandSize = 2;
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineI8:
                case OperandType.InlineMethod:
                case OperandType.InlineR:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineSwitch:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                case OperandType.InlineTok:
                    operandSize = 4;
                    break;
            }
            if (insn.operand is Array array)
            {
                operandSize *= array.Length;
                operandSize += 4;
            }
            return operandSize;
        }

        /// <summary>
        /// Returns the opcode with the reverse conditional behavior as the one given, I.E. ble instead of bgt.
        /// </summary>
        public static OpCode GetReverseConditionalCode(OpCode code)
        {
            return reversedConditionalCodes.TryGetValue(code.Value, out OpCode reversedCode) ? reversedCode : code;
        }

        /// <summary>
        /// Creates a new <see cref="CILProcessor"/> based upon the given instructions.
        /// </summary>
        public static CILProcessor CreateProcessor(IEnumerable<CodeInstruction> insns, ILGenerator gen)
        {
            return new CILProcessor(insns, gen);
        }

        /// <summary>
        /// Represents a block of <see cref="CodeInstruction"/>s. Tracks changes, and provides data for various helper functions.
        /// </summary>
        public class CILProcessor
        {
            /// <summary>
            /// The workable list of <see cref="CodeInstruction"/>s
            /// </summary>
            public List<CodeInstruction> Insns;
            /// <summary>
            /// The <see cref="CodeInstruction"/>s at the moment this analysis was made.
            /// </summary>
            public List<CodeInstruction> BaseInsns;
            /// <summary>
            /// Tracks all of the labels in the CodeInstructions, where <see cref="Tuple{Int32, Int32}.Item1"/> is the label number, and <see cref="Tuple{Int32, Int32}.Item2"/> is the instruction index.
            /// </summary>
            public Dictionary<Label, Tuple<int, int>> LabelIndexes;
            /// <summary>
            /// Contains the addresses of each IL instruction by index.
            /// </summary>
            public List<int> ILAddresses;
            /// <summary>
            /// Contains the index of each IL instruction by address.
            /// </summary>
            public Dictionary<int, int> ILIndexes;
            /// <summary>
            /// The <see cref="ILGenerator"/> available for use by this CILProcessor.
            /// </summary>
            public readonly ILGenerator ILGen;

            private Dictionary<int, ILRef> ILRefs;

            internal CILProcessor(IEnumerable<CodeInstruction> insns, ILGenerator gen)
            {
                if (!insns.Any()) throw new InvalidOperationException("Cannot process empty CodeInstruction enumerable!");
                ILGen = gen;
                Insns = insns.ToList();
                BaseInsns = insns.Select(x => x.Clone()).ToList();
                ILRefs = new Dictionary<int, ILRef>();
                UpdateMetadata();
            }

            private void UpdateMetadata()
            {
                int labels = 0;
                LabelIndexes = new Dictionary<Label, Tuple<int, int>>();
                ILAddresses = new List<int>();
                ILIndexes = new Dictionary<int, int>();
                ILAddresses.Add(0);
                ILIndexes.Add(0, 0);
                foreach (CodeInstruction insn in Insns)
                {
                    if (insn == null || insn.labels == null) continue;
                    ILAddresses.Add(ILAddresses[ILAddresses.Count - 1] + insn.opcode.Size + GetOperandSize(insn));
                    ILIndexes[ILAddresses[ILAddresses.Count - 1]] = ILAddresses.Count - 1;
                    foreach (Label label in insn.labels)
                    {
                        LabelIndexes.Add(label, Tuple.Create(++labels, ILAddresses.Count - 2));
                    }
                }
            }

            /// <summary>
            /// Returns an <see cref="ILRef"/> for the given instruction index.
            /// </summary>
            public ILRef GetRefByIndex(int index)
            {
                if (index < 0 || index >= Insns.Count) throw new InvalidOperationException("IL index invalid!");
                return ILRefs.ContainsKey(index) ? ILRefs[index] : new ILRef(this, index);
            }

            /// <summary>
            /// Returns an <see cref="ILRef"/> for the given instruction address.
            /// </summary>
            public ILRef GetRefByAddress(int address, bool fuzzy = false)
            {
                if (fuzzy)
                {
                    if (address < 0 || address > ILAddresses[ILAddresses.Count - 1] + Insns[Insns.Count - 1].opcode.Size + GetOperandSize(Insns[Insns.Count - 1])) throw new InvalidOperationException("IL address invalid!");
                    for (; !ILIndexes.ContainsKey(address); address--) if (address < 0) throw new Exception("Unknown Error finding reference to IL address!");
                    return ILRefs.ContainsKey(ILIndexes[address]) ? ILRefs[ILIndexes[address]] : new ILRef(this, ILIndexes[address]);
                }
                else
                {
                    if (!ILIndexes.ContainsKey(address)) throw new InvalidOperationException("IL address invalid!");
                    return ILRefs.ContainsKey(ILIndexes[address]) ? ILRefs[ILIndexes[address]] : new ILRef(this, ILIndexes[address]);
                }
            }

            /// <summary>
            /// Returns an <see cref="ILRef"/> for the instruction with its index offset from the given <see cref="ILRef"/>
            /// </summary>
            public ILRef GetRefByOffset(ILRef current, int offset)
            {
                return GetRefByIndex(current.Index + offset);
            }

            /// <summary>
            /// Finds all instructions that match the given instruction's opcode and operand.
            /// Ignores the instruction's labels and exception blocks.
            /// </summary>
            public ILRef[] FindAllRefsByInsn(CodeInstruction insn)
            {
                if (insn == null) throw new ArgumentNullException("insn");
                bool hasToString = insn.operand != null && insn.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object));
                return Insns.Select((x, i) => Tuple.Create(x, i)).Where(x => x.Item1?.opcode == insn.opcode && hasToString ? x.Item1?.operand?.ToString() == insn.operand?.ToString() : x.Item1?.operand == insn.operand).Select(x => GetRefByIndex(x.Item2)).ToArray();
            }

            /// <summary>
            /// Finds the first instruction that matches the given instruction's opcode and operand.
            /// Ignores the instruction's labels and exception blocks.
            /// </summary>
            public ILRef FindRefByInsn(CodeInstruction insn)
            {
                if (insn == null) throw new ArgumentNullException("insn");
                bool hasToString = insn.operand != null && insn.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object));
                int index = Insns.FindIndex(x => x?.opcode == insn.opcode && hasToString ? x?.operand?.ToString() == insn.operand?.ToString() : x?.operand == insn.operand);
                return index >= 0 ? GetRefByIndex(index) : null;
            }

            /// <summary>
            /// Finds the last instruction that matches the given instruction's opcode and operand.
            /// Ignores the instruction's labels and exception blocks.
            /// </summary>
            public ILRef FindLastRefByInsn(CodeInstruction insn)
            {
                if (insn == null) throw new ArgumentNullException("insn");
                bool hasToString = insn.operand != null && insn.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object));
                int index = Insns.FindLastIndex(x => x?.opcode == insn.opcode && hasToString ? x?.operand?.ToString() == insn.operand?.ToString() : x?.operand == insn.operand);
                return index >= 0 ? GetRefByIndex(index) : null;
            }

            /// <summary>
            /// Finds all instructions that match the given instructions' opcodes and operands.
            /// Ignores the instructions' labels and exception blocks.
            /// The <see cref="ILRef"/> points to the first instruction in the block.
            /// </summary>
            public ILRef[] FindAllRefsByInsns(List<CodeInstruction> searchInsns)
            {
                if (searchInsns == null) throw new ArgumentNullException("searchInsns");
                EqualityComparison<CodeInstruction>[] comparers = searchInsns.Select(x => x == null ? (a, b) => true : x?.operand != null && x.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object)) ? (a, b) => a?.opcode == b?.opcode && a?.operand?.ToString() == b?.operand?.ToString() : (EqualityComparison<CodeInstruction>)((a, b) => a?.opcode == b?.opcode && a?.operand == b?.operand)).ToArray();
                return Insns.AllIndexesOfSublist(searchInsns.ToList(), 0, comparers).Select(x => GetRefByIndex(x)).ToArray();
            }

            /// <summary>
            /// Finds the first instruction block that matches the given instructions' opcodes and operands.
            /// Ignores the instruction's labels and exception blocks.
            /// The <see cref="ILRef"/> points to the first instruction in the block.
            /// </summary>
            public ILRef FindRefByInsns(CodeInstruction[] searchInsns)
            {
                if (searchInsns == null) throw new ArgumentNullException("searchInsns");
                EqualityComparison<CodeInstruction>[] comparers = searchInsns.Select(x => x == null ? (a, b) => true : x?.operand != null && x.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object)) ? (a, b) => a?.opcode == b?.opcode && a?.operand?.ToString() == b?.operand?.ToString() : (EqualityComparison<CodeInstruction>)((a, b) => a?.opcode == b?.opcode && a?.operand == b?.operand)).ToArray();
                return GetRefByIndex(Insns.IndexOfSublist(searchInsns.ToList(), 0, comparers));
            }

            /// <summary>
            /// Finds the last instruction block that matches the given instructions' opcodes and operands.
            /// Ignores the instruction's labels and exception blocks.
            /// The <see cref="ILRef"/> points to the first instruction in the block.
            /// </summary>
            public ILRef FindLastRefByInsns(CodeInstruction[] searchInsns)
            {
                if (searchInsns == null) throw new ArgumentNullException("searchInsns");
                EqualityComparison<CodeInstruction>[] comparers = searchInsns.Select(x => x == null ? (a, b) => true : x?.operand != null && x.operand.GetType().GetMethods().Any(m => m.Name == "ToString" && m.GetParameters().Length == 0 && m.DeclaringType != typeof(object)) ? (a, b) => a?.opcode == b?.opcode && a?.operand?.ToString() == b?.operand?.ToString() : (EqualityComparison<CodeInstruction>)((a, b) => a?.opcode == b?.opcode && a?.operand == b?.operand)).ToArray();
                return GetRefByIndex(Insns.LastIndexOfSublist(searchInsns.ToList(), 0, comparers));
            }

            /// <summary>
            /// Finds all instructions that have the given opcode.
            /// </summary>
            public ILRef[] FindAllRefsByOpCode(OpCode code)
            {
                if (code == null) throw new ArgumentNullException("code");
                return Insns.Select((x, i) => Tuple.Create(x, i)).Where(x => x.Item1?.opcode == code).Select(x => GetRefByIndex(x.Item2)).ToArray();
            }

            /// <summary>
            /// Finds the first instruction that has the given opcode.
            /// </summary>
            public ILRef FindRefByOpCode(OpCode code)
            {
                if (code == null) throw new ArgumentNullException("code");
                int index = Insns.FindIndex(x => x?.opcode == code);
                return index >= 0 ? GetRefByIndex(index) : null;
            }

            /// <summary>
            /// Finds the last instruction that has the given opcode.
            /// </summary>
            public ILRef FindLastRefByOpCode(OpCode code)
            {
                if (code == null) throw new ArgumentNullException("code");
                int index = Insns.FindLastIndex(x => x?.opcode == code);
                return index >= 0 ? GetRefByIndex(index) : null;
            }

            /// <summary>
            /// Finds the instruction with the given label. Returns null if no instruction has the given label.
            /// Returns the first instruction found with the given label - if multiple instructions have the same label, then that is an invalid state.
            /// </summary>
            public ILRef FindRefByLabel(Label label)
            {
                int index = Insns.FindIndex(x => x.labels.Contains(label));
                return index >= 0 ? GetRefByIndex(index) : null;
            }

            /// <summary>
            /// Dumps the instructions of this <see cref="CILProcessor"/> as a string,
            /// optionally with a given starting <see cref="ILRef"/> and count of instructions to print.
            /// lines are seperated by the \n character.
            /// </summary>
            public string DumpInstructions(ILRef start, int count = int.MaxValue)
            {
                if (!start.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                return DumpInstructions(start?.Index ?? 0, count);
            }

            /// <summary>
            /// Dumps the instructions of this <see cref="CILProcessor"/> as a string,
            /// optionally with a given starting instruction index and count of instructions to print.
            /// lines are seperated by the \n character.
            /// </summary>
            public string DumpInstructions(int start = 0, int count = int.MaxValue)
            {
                if (start < 0 || count <= 0) return string.Empty;
                StringBuilder sb = new StringBuilder();
                int insnsToSkip = start;
                int insnsToDump = count;
                int totalInsns = Insns.Count;
                int lineNum = 0;
                foreach (CodeInstruction insn in Insns)
                {
                    lineNum++;
                    if (insnsToSkip > 0)
                    {
                        insnsToSkip--;
                        continue;
                    }
                    if (insnsToDump <= 0) break;
                    if (insnsToDump < count) sb.Append('\n');
                    if (insn != null)
                    {
                        sb.AppendFormat("LINE {0}: IL_{1}: {2,-10}{3}",
                            lineNum.ToString().PadLeft((int)Math.Log10(totalInsns) + 1, '0'),
                            ILAddresses[lineNum - 1].ToString("X").PadLeft((int)Math.Log(ILAddresses[totalInsns - 1], 16) + 1, '0'),
                            insn.opcode.ToString(),
                            insn.operand is Label label ? LabelToString(label) :
                            insn.operand is string ? "\"" + insn.operand + "\"" :
                            insn.operand is Label[] array ? "[" + array.Select(x => LabelToString(x)).Concat() + "]" :
                            insn.operand?.ToString() ?? string.Empty);
                    }
                    else
                    {
                        sb.AppendFormat("LINE {0}: NULL INSN", lineNum.ToString().PadLeft((int)Math.Log10(totalInsns) + 1, '0'));
                    }
                    insnsToDump--;
                }
                return sb.ToString();

                string LabelToString(Label label)
                {
                    return LabelIndexes.ContainsKey(label) ?
                        "LABEL " + LabelIndexes[label].Item1 +
                        "; LINE " + (LabelIndexes[label].Item2 + 1) +
                        "; IL_" + ILAddresses[LabelIndexes[label].Item2].ToString("X").PadLeft((int)Math.Log(ILAddresses[totalInsns - 1], 16) + 1, '0') :
                        "ERROR: LABEL NOT FOUND!";
                }
            }

            /// <summary>
            /// Gets the <see cref="CodeInstruction"/> referenced by the given <see cref="ILRef"/>
            /// </summary>
            public CodeInstruction GetInsn(ILRef target)
            {
                return Insns[target.Index];
            }

            /// <summary>
            /// Simplifies needlessly complicated conditional branch structures.
            /// </summary>
            public ILRef SimplifyConditionalBranch(ILRef ifBranch)
            {
                if (ifBranch == null) throw new ArgumentNullException("ifBranch");
                if (!ifBranch.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                CodeInstruction conditionalBranch = GetInsn(ifBranch);
                CodeInstruction nonconditionalBranch = GetInsn(ifBranch.GetRefByOffset(1));
                if (conditionalBranch.opcode.FlowControl == FlowControl.Cond_Branch && nonconditionalBranch.opcode.FlowControl == FlowControl.Branch &&
                    GetInsn(ifBranch.GetRefByOffset(2)).labels.Contains((Label)conditionalBranch.operand) &&
                    conditionalBranch.opcode != GetReverseConditionalCode(conditionalBranch.opcode))
                {
                    conditionalBranch.opcode = GetReverseConditionalCode(conditionalBranch.opcode);
                    conditionalBranch.operand = nonconditionalBranch.operand;
                    RemoveInsn(ifBranch.GetRefByOffset(1));
                }
                return ifBranch;
            }

            /// <summary>
            /// Injects an elseif structure onto the end of a specified if block specified by the <see cref="ILRef"/> that is targeting a conditional branch instruction.
            /// The returned <see cref="ILRef"/>s will point to NOPs you can use and/or replace.
            /// </summary>
            public void InjectElseIf(ILRef ifBranch, OpCode conditionCode, out ILRef conditionBlock, out ILRef codeBlock)
            {
                if (ifBranch == null) throw new ArgumentNullException("ifBranch");
                if (!ifBranch.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                CodeInstruction brCode = GetInsn(ifBranch);
                if (brCode.opcode.FlowControl != FlowControl.Cond_Branch) throw new InvalidOperationException("ifBranch must point to a conditional branch instruction!");
                Label exitLabel = (Label)brCode.operand;
                Label conditionLabel = ILGen.DefineLabel();
                brCode.operand = conditionLabel;
                ILRef exitRef = FindRefByLabel(exitLabel);
                InjectInsn(exitRef, new CodeInstruction(OpCodes.Br, exitLabel), true);
                CodeInstruction conditionInsn = new CodeInstruction(conditionCode, exitLabel);
                conditionBlock = InjectInsn(exitRef, new CodeInstruction(OpCodes.Nop), true);
                GetInsn(conditionBlock).labels.Add(conditionLabel);
                InjectInsn(exitRef, conditionInsn, true);
                codeBlock = InjectInsn(exitRef, new CodeInstruction(OpCodes.Nop), true);
            }

            /// <summary>
            /// Injects a CALL instruction into the given <see cref="ILRef"/>. By default, it will replace the target instruction.
            /// If <paramref name="insert"/> is true, then it will insert the new instruction and push forward the instructions at
            /// and after the given <see cref="ILRef"/>
            /// </summary>
            public ILRef InjectHook(ILRef target, MethodBase hook, bool insert = true)
            {
                if (target == null) throw new ArgumentNullException("target");
                if (hook == null) throw new ArgumentNullException("hook");
                if (!target.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                int targetIndex = target.Index;
                CodeInstruction targetInsn = GetInsn(target);
                if (insert) ShiftInsns(target, 1);
                Insns[targetIndex] = new CodeInstruction(hook.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, hook);
                Insns[targetIndex].labels?.AddRange(targetInsn.labels);
                return insert ? GetRefByIndex(targetIndex) : target;
            }

            /// <summary>
            /// Injects an instruction into the given <see cref="ILRef"/>. By default, it will replace the target instruction.
            /// If <paramref name="insert"/> is true, then it will insert the new instruction and push forward the instructions at
            /// and after the given <see cref="ILRef"/>
            /// </summary>
            public ILRef InjectInsn(ILRef target, CodeInstruction insn, bool insert = true)
            {
                if (target == null) throw new ArgumentNullException("target");
                if (insn == null) throw new ArgumentNullException("insn");
                if (!target.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                int targetIndex = target.Index;
                CodeInstruction targetInsn = GetInsn(target);
                insn.labels?.AddRange(targetInsn.labels);
                if (insert) ShiftInsns(target, 1);
                Insns[target.Index] = new CodeInstruction(insn);
                return insert ? GetRefByIndex(targetIndex) : target;
            }

            /// <summary>
            /// Injects a set of instructions into the block started by the given <see cref="ILRef"/>. By default, it will replace the target instructions.
            /// If <paramref name="insert"/> is true, then it will insert the new instructions and push forward the instructions at
            /// and after the given <see cref="ILRef"/>
            /// </summary>
            public ILRef InjectInsns(ILRef target, CodeInstruction[] newInsns, bool insert = true)
            {
                if (target == null) throw new ArgumentNullException("target");
                if (newInsns == null) throw new ArgumentNullException("newInsns");
                if (newInsns.Length < 1) throw new ArgumentOutOfRangeException("newInsns");
                if (!target.Valid) throw new InvalidOperationException("Cannot use an invalid ILRef!");
                int targetIndex = target.Index;
                CodeInstruction targetInsn = GetInsn(target);
                newInsns[0]?.labels?.AddRange(targetInsn.labels);
                if (targetInsn.opcode == OpCodes.Nop) insert = false;
                if (insert) ShiftInsns(target, newInsns.Length);
                if (Insns.Capacity < targetIndex + newInsns.Length) Insns.Capacity = Math.Max(targetIndex + newInsns.Length, (int)Math.Pow(2, Math.Floor(Math.Log(Insns.Capacity, 2) + 1)));
                for (int i = targetIndex; i < targetIndex + newInsns.Length;i++)
                {
                    newInsns[i - targetIndex].labels.AddRange(Insns[i].labels);
                    Insns[i] = newInsns[i - targetIndex];
                }
                return insert ? GetRefByIndex(targetIndex) : target;
            }

            /// <summary>
            /// Removes the instruction at the given <see cref="ILRef"/>.
            /// </summary>
            public void RemoveInsn(ILRef target)
            {
                RemoveInsns(target, 1);
            }

            /// <summary>
            /// Removes the set of instructions in the block started by the given <see cref="ILRef"/>.
            /// Accepts values of <paramref name="count"/> higher than the number of instructions.
            /// </summary>
            public void RemoveInsns(ILRef target, int count)
            {
                ShiftInsns(target.GetRefByOffset(count), -count);
            }

            private void ShiftInsns(ILRef target, int shift)
            {
                if (shift == 0) return;
                int targetIndex = target.Index;
                if (shift > 0)
                {
                    Insns.InsertRange(targetIndex, new CodeInstruction[shift]);
                    UpdateMetadata();
                    foreach (KeyValuePair<int, ILRef> r in ILRefs.Where(x => x.Key >= targetIndex).OrderByDescending(x => x.Key).ToList())
                    {
                        r.Value.Shift(shift);
                        ILRefs[r.Key + shift] = r.Value;
                        ILRefs.Remove(r.Key);
                    }
                }
                else if (target.Index + shift >= 0)
                {
                    Insns.RemoveRange(targetIndex + shift, -shift);
                    Insns.TrimExcess();
                    UpdateMetadata();
                    foreach (KeyValuePair<int, ILRef> r in ILRefs.Where(x => x.Key >= targetIndex + shift).OrderBy(x => x.Key).ToList())
                    {
                        if (r.Key >= targetIndex)
                        {
                            r.Value.Shift(shift);
                            ILRefs[r.Key + shift] = r.Value;
                            ILRefs.Remove(r.Key);
                        }
                        else r.Value.Invalidate();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot shift past the bottom of the insn list!");
                }
            }

            /// <summary>
            /// Returns the complete list of <see cref="CodeInstruction"/>s.
            /// </summary>
            public List<CodeInstruction> GetInstructions()
            {
                return Insns;
            }

            /// <summary>
            /// Converts this <see cref="CILProcessor"/> into its complete list of <see cref="CodeInstruction"/>s.
            /// </summary>
            public static implicit operator List<CodeInstruction>(CILProcessor p)
            {
                return p.GetInstructions();
            }

            /// <summary>
            /// Represents a reference to a specific IL address.
            /// </summary>
            public class ILRef
            {
                private CILProcessor processor;
                /// <summary>
                /// The index of the referenced instruction.
                /// </summary>
                public int Index { get; private set; }
                /// <summary>
                /// The address of the referenced instruction.
                /// </summary>
                public int Address
                {
                    get
                    {
                        return processor.ILAddresses[Index];
                    }
                }
                /// <summary>
                /// Indicates whether this <see cref="ILRef"/> is still valid - The reference will be invalidated if the instruction it references is removed.
                /// </summary>
                public bool Valid { get; private set; } = true;
                internal ILRef(CILProcessor processor, int index)
                {
                    this.processor = processor;
                    Index = index;
                    processor.ILRefs.Add(index, this);
                }

                internal void Invalidate()
                { 
                    Valid = false;
                    processor.ILRefs.Remove(Index);
                }

                internal void Shift(int shift)
                {
                    Index += shift;
                }

                /// <summary>
                /// Returns an <see cref="ILRef"/> for the instruction with its index offset from the given <see cref="ILRef"/>
                /// </summary>
                public ILRef GetRefByOffset(int offset)
                {
                    return processor.GetRefByOffset(this, offset);
                }
            }
        }
    }
}
