using System.Collections.Generic;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000BF RID: 191
	internal class SourceMethodBuilder
	{
		// Token: 0x06000683 RID: 1667 RVA: 0x00029750 File Offset: 0x00027950
		public SourceMethodBuilder(ICompileUnit comp_unit)
		{
			this._comp_unit = comp_unit;
			this.method_lines = new List<LineNumberEntry>();
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0002976C File Offset: 0x0002796C
		public SourceMethodBuilder(ICompileUnit comp_unit, int ns_id, IMethodDef method) : this(comp_unit)
		{
			this.ns_id = ns_id;
			this.method = method;
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x00029788 File Offset: 0x00027988
		public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, bool is_hidden)
		{
			this.MarkSequencePoint(offset, file, line, column, -1, -1, is_hidden);
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0002979C File Offset: 0x0002799C
		public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, int end_line, int end_column, bool is_hidden)
		{
			int file2 = (file != null) ? file.Index : 0;
			LineNumberEntry lineNumberEntry = new LineNumberEntry(file2, line, column, end_line, end_column, offset, is_hidden);
			bool flag = this.method_lines.Count > 0;
			if (flag)
			{
				LineNumberEntry lineNumberEntry2 = this.method_lines[this.method_lines.Count - 1];
				bool flag2 = lineNumberEntry2.Offset == offset;
				if (flag2)
				{
					bool flag3 = LineNumberEntry.LocationComparer.Default.Compare(lineNumberEntry, lineNumberEntry2) > 0;
					if (flag3)
					{
						this.method_lines[this.method_lines.Count - 1] = lineNumberEntry;
					}
					return;
				}
			}
			this.method_lines.Add(lineNumberEntry);
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x00029844 File Offset: 0x00027A44
		public void StartBlock(CodeBlockEntry.Type type, int start_offset)
		{
			this.StartBlock(type, start_offset, (this._blocks == null) ? 1 : (this._blocks.Count + 1));
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00029868 File Offset: 0x00027A68
		public void StartBlock(CodeBlockEntry.Type type, int start_offset, int scopeIndex)
		{
			bool flag = this._block_stack == null;
			if (flag)
			{
				this._block_stack = new Stack<CodeBlockEntry>();
			}
			bool flag2 = this._blocks == null;
			if (flag2)
			{
				this._blocks = new List<CodeBlockEntry>();
			}
			int parent = (this.CurrentBlock != null) ? this.CurrentBlock.Index : -1;
			CodeBlockEntry item = new CodeBlockEntry(scopeIndex, parent, type, start_offset);
			this._block_stack.Push(item);
			this._blocks.Add(item);
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x000298E4 File Offset: 0x00027AE4
		public void EndBlock(int end_offset)
		{
			CodeBlockEntry codeBlockEntry = this._block_stack.Pop();
			codeBlockEntry.Close(end_offset);
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x00029908 File Offset: 0x00027B08
		public CodeBlockEntry[] Blocks
		{
			get
			{
				bool flag = this._blocks == null;
				CodeBlockEntry[] result;
				if (flag)
				{
					result = new CodeBlockEntry[0];
				}
				else
				{
					CodeBlockEntry[] array = new CodeBlockEntry[this._blocks.Count];
					this._blocks.CopyTo(array, 0);
					result = array;
				}
				return result;
			}
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x0600068B RID: 1675 RVA: 0x00029950 File Offset: 0x00027B50
		public CodeBlockEntry CurrentBlock
		{
			get
			{
				bool flag = this._block_stack != null && this._block_stack.Count > 0;
				CodeBlockEntry result;
				if (flag)
				{
					result = this._block_stack.Peek();
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x0600068C RID: 1676 RVA: 0x00029990 File Offset: 0x00027B90
		public LocalVariableEntry[] Locals
		{
			get
			{
				bool flag = this._locals == null;
				LocalVariableEntry[] result;
				if (flag)
				{
					result = new LocalVariableEntry[0];
				}
				else
				{
					result = this._locals.ToArray();
				}
				return result;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x0600068D RID: 1677 RVA: 0x000299C4 File Offset: 0x00027BC4
		public ICompileUnit SourceFile
		{
			get
			{
				return this._comp_unit;
			}
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x000299DC File Offset: 0x00027BDC
		public void AddLocal(int index, string name)
		{
			bool flag = this._locals == null;
			if (flag)
			{
				this._locals = new List<LocalVariableEntry>();
			}
			int block = (this.CurrentBlock != null) ? this.CurrentBlock.Index : 0;
			this._locals.Add(new LocalVariableEntry(index, name, block));
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x0600068F RID: 1679 RVA: 0x00029A30 File Offset: 0x00027C30
		public ScopeVariable[] ScopeVariables
		{
			get
			{
				bool flag = this._scope_vars == null;
				ScopeVariable[] result;
				if (flag)
				{
					result = new ScopeVariable[0];
				}
				else
				{
					result = this._scope_vars.ToArray();
				}
				return result;
			}
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x00029A64 File Offset: 0x00027C64
		public void AddScopeVariable(int scope, int index)
		{
			bool flag = this._scope_vars == null;
			if (flag)
			{
				this._scope_vars = new List<ScopeVariable>();
			}
			this._scope_vars.Add(new ScopeVariable(scope, index));
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00029AA0 File Offset: 0x00027CA0
		public void DefineMethod(MonoSymbolFile file)
		{
			this.DefineMethod(file, this.method.Token);
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00029AB8 File Offset: 0x00027CB8
		public void DefineMethod(MonoSymbolFile file, int token)
		{
			CodeBlockEntry[] array = this.Blocks;
			bool flag = array.Length != 0;
			if (flag)
			{
				List<CodeBlockEntry> list = new List<CodeBlockEntry>(array.Length);
				int num = 0;
				for (int i = 0; i < array.Length; i++)
				{
					num = System.Math.Max(num, array[i].Index);
				}
				for (int j = 0; j < num; j++)
				{
					int num2 = j + 1;
					bool flag2 = j < array.Length && array[j].Index == num2;
					if (flag2)
					{
						list.Add(array[j]);
					}
					else
					{
						bool flag3 = false;
						for (int k = 0; k < array.Length; k++)
						{
							bool flag4 = array[k].Index == num2;
							if (flag4)
							{
								list.Add(array[k]);
								flag3 = true;
								break;
							}
						}
						bool flag5 = flag3;
						if (!flag5)
						{
							list.Add(new CodeBlockEntry(num2, -1, CodeBlockEntry.Type.CompilerGenerated, 0));
						}
					}
				}
				array = list.ToArray();
			}
			MethodEntry entry = new MethodEntry(file, this._comp_unit.Entry, token, this.ScopeVariables, this.Locals, this.method_lines.ToArray(), array, null, MethodEntry.Flags.ColumnsInfoIncluded, this.ns_id);
			file.AddMethod(entry);
		}

		// Token: 0x040003F5 RID: 1013
		private List<LocalVariableEntry> _locals;

		// Token: 0x040003F6 RID: 1014
		private List<CodeBlockEntry> _blocks;

		// Token: 0x040003F7 RID: 1015
		private List<ScopeVariable> _scope_vars;

		// Token: 0x040003F8 RID: 1016
		private Stack<CodeBlockEntry> _block_stack;

		// Token: 0x040003F9 RID: 1017
		private readonly List<LineNumberEntry> method_lines;

		// Token: 0x040003FA RID: 1018
		private readonly ICompileUnit _comp_unit;

		// Token: 0x040003FB RID: 1019
		private readonly int ns_id;

		// Token: 0x040003FC RID: 1020
		private readonly IMethodDef method;
	}
}
