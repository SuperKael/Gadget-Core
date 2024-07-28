using System;
using System.Collections.Generic;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000BB RID: 187
	internal class MethodEntry : IComparable
	{
		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000655 RID: 1621 RVA: 0x0002871C File Offset: 0x0002691C
		public MethodEntry.Flags MethodFlags
		{
			get
			{
				return this.flags;
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000656 RID: 1622 RVA: 0x00028734 File Offset: 0x00026934
		// (set) Token: 0x06000657 RID: 1623 RVA: 0x0002874C File Offset: 0x0002694C
		public int Index
		{
			get
			{
				return this.index;
			}
			set
			{
				this.index = value;
			}
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x00028758 File Offset: 0x00026958
		internal MethodEntry(MonoSymbolFile file, MyBinaryReader reader, int index)
		{
			this.SymbolFile = file;
			this.index = index;
			this.Token = reader.ReadInt32();
			this.DataOffset = reader.ReadInt32();
			this.LineNumberTableOffset = reader.ReadInt32();
			long position = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)this.DataOffset;
			this.CompileUnitIndex = reader.ReadLeb128();
			this.LocalVariableTableOffset = reader.ReadLeb128();
			this.NamespaceID = reader.ReadLeb128();
			this.CodeBlockTableOffset = reader.ReadLeb128();
			this.ScopeVariableTableOffset = reader.ReadLeb128();
			this.RealNameOffset = reader.ReadLeb128();
			this.flags = (MethodEntry.Flags)reader.ReadLeb128();
			reader.BaseStream.Position = position;
			this.CompileUnit = file.GetCompileUnit(this.CompileUnitIndex);
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x00028834 File Offset: 0x00026A34
		internal MethodEntry(MonoSymbolFile file, CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, MethodEntry.Flags flags, int namespace_id)
		{
			this.SymbolFile = file;
			this.real_name = real_name;
			this.locals = locals;
			this.code_blocks = code_blocks;
			this.scope_vars = scope_vars;
			this.flags = flags;
			this.index = -1;
			this.Token = token;
			this.CompileUnitIndex = comp_unit.Index;
			this.CompileUnit = comp_unit;
			this.NamespaceID = namespace_id;
			MethodEntry.CheckLineNumberTable(lines);
			this.lnt = new LineNumberTable(file, lines);
			file.NumLineNumbers += lines.Length;
			int num = (locals != null) ? locals.Length : 0;
			bool flag = num <= 32;
			if (flag)
			{
				for (int i = 0; i < num; i++)
				{
					string name = locals[i].Name;
					for (int j = i + 1; j < num; j++)
					{
						bool flag2 = locals[j].Name == name;
						if (flag2)
						{
							flags |= MethodEntry.Flags.LocalNamesAmbiguous;
							goto IL_FF;
						}
					}
				}
				IL_FF:;
			}
			else
			{
				Dictionary<string, LocalVariableEntry> dictionary = new Dictionary<string, LocalVariableEntry>();
				foreach (LocalVariableEntry localVariableEntry in locals)
				{
					bool flag3 = dictionary.ContainsKey(localVariableEntry.Name);
					if (flag3)
					{
						flags |= MethodEntry.Flags.LocalNamesAmbiguous;
						break;
					}
					dictionary.Add(localVariableEntry.Name, localVariableEntry);
				}
			}
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x000289A4 File Offset: 0x00026BA4
		private static void CheckLineNumberTable(LineNumberEntry[] line_numbers)
		{
			int num = -1;
			int num2 = -1;
			bool flag = line_numbers == null;
			if (!flag)
			{
				foreach (LineNumberEntry lineNumberEntry in line_numbers)
				{
					bool flag2 = lineNumberEntry.Equals(LineNumberEntry.Null);
					if (flag2)
					{
						throw new MonoSymbolFileException();
					}
					bool flag3 = lineNumberEntry.Offset < num;
					if (flag3)
					{
						throw new MonoSymbolFileException();
					}
					bool flag4 = lineNumberEntry.Offset > num;
					if (flag4)
					{
						num2 = lineNumberEntry.Row;
						num = lineNumberEntry.Offset;
					}
					else
					{
						bool flag5 = lineNumberEntry.Row > num2;
						if (flag5)
						{
							num2 = lineNumberEntry.Row;
						}
					}
				}
			}
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x00028A50 File Offset: 0x00026C50
		internal void Write(MyBinaryWriter bw)
		{
			bool flag = this.index <= 0 || this.DataOffset == 0;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			bw.Write(this.Token);
			bw.Write(this.DataOffset);
			bw.Write(this.LineNumberTableOffset);
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x00028AA4 File Offset: 0x00026CA4
		internal void WriteData(MonoSymbolFile file, MyBinaryWriter bw)
		{
			bool flag = this.index <= 0;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			this.LocalVariableTableOffset = (int)bw.BaseStream.Position;
			int num = (this.locals != null) ? this.locals.Length : 0;
			bw.WriteLeb128(num);
			for (int i = 0; i < num; i++)
			{
				this.locals[i].Write(file, bw);
			}
			file.LocalCount += num;
			this.CodeBlockTableOffset = (int)bw.BaseStream.Position;
			int num2 = (this.code_blocks != null) ? this.code_blocks.Length : 0;
			bw.WriteLeb128(num2);
			for (int j = 0; j < num2; j++)
			{
				this.code_blocks[j].Write(bw);
			}
			this.ScopeVariableTableOffset = (int)bw.BaseStream.Position;
			int num3 = (this.scope_vars != null) ? this.scope_vars.Length : 0;
			bw.WriteLeb128(num3);
			for (int k = 0; k < num3; k++)
			{
				this.scope_vars[k].Write(bw);
			}
			bool flag2 = this.real_name != null;
			if (flag2)
			{
				this.RealNameOffset = (int)bw.BaseStream.Position;
				bw.Write(this.real_name);
			}
			foreach (LineNumberEntry lineNumberEntry in this.lnt.LineNumbers)
			{
				bool flag3 = lineNumberEntry.EndRow != -1 || lineNumberEntry.EndColumn != -1;
				if (flag3)
				{
					this.flags |= MethodEntry.Flags.EndInfoIncluded;
				}
			}
			this.LineNumberTableOffset = (int)bw.BaseStream.Position;
			this.lnt.Write(file, bw, (this.flags & MethodEntry.Flags.ColumnsInfoIncluded) > (MethodEntry.Flags)0, (this.flags & MethodEntry.Flags.EndInfoIncluded) > (MethodEntry.Flags)0);
			this.DataOffset = (int)bw.BaseStream.Position;
			bw.WriteLeb128(this.CompileUnitIndex);
			bw.WriteLeb128(this.LocalVariableTableOffset);
			bw.WriteLeb128(this.NamespaceID);
			bw.WriteLeb128(this.CodeBlockTableOffset);
			bw.WriteLeb128(this.ScopeVariableTableOffset);
			bw.WriteLeb128(this.RealNameOffset);
			bw.WriteLeb128((int)this.flags);
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x00028D08 File Offset: 0x00026F08
		public void ReadAll()
		{
			this.GetLineNumberTable();
			this.GetLocals();
			this.GetCodeBlocks();
			this.GetScopeVariables();
			this.GetRealName();
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x00028D30 File Offset: 0x00026F30
		public LineNumberTable GetLineNumberTable()
		{
			MonoSymbolFile symbolFile = this.SymbolFile;
			LineNumberTable result;
			lock (symbolFile)
			{
				bool flag = this.lnt != null;
				if (flag)
				{
					result = this.lnt;
				}
				else
				{
					bool flag2 = this.LineNumberTableOffset == 0;
					if (flag2)
					{
						result = null;
					}
					else
					{
						MyBinaryReader binaryReader = this.SymbolFile.BinaryReader;
						long position = binaryReader.BaseStream.Position;
						binaryReader.BaseStream.Position = (long)this.LineNumberTableOffset;
						this.lnt = LineNumberTable.Read(this.SymbolFile, binaryReader, (this.flags & MethodEntry.Flags.ColumnsInfoIncluded) > (MethodEntry.Flags)0, (this.flags & MethodEntry.Flags.EndInfoIncluded) > (MethodEntry.Flags)0);
						binaryReader.BaseStream.Position = position;
						result = this.lnt;
					}
				}
			}
			return result;
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x00028E00 File Offset: 0x00027000
		public LocalVariableEntry[] GetLocals()
		{
			MonoSymbolFile symbolFile = this.SymbolFile;
			LocalVariableEntry[] result;
			lock (symbolFile)
			{
				bool flag = this.locals != null;
				if (flag)
				{
					result = this.locals;
				}
				else
				{
					bool flag2 = this.LocalVariableTableOffset == 0;
					if (flag2)
					{
						result = null;
					}
					else
					{
						MyBinaryReader binaryReader = this.SymbolFile.BinaryReader;
						long position = binaryReader.BaseStream.Position;
						binaryReader.BaseStream.Position = (long)this.LocalVariableTableOffset;
						int num = binaryReader.ReadLeb128();
						this.locals = new LocalVariableEntry[num];
						for (int i = 0; i < num; i++)
						{
							this.locals[i] = new LocalVariableEntry(this.SymbolFile, binaryReader);
						}
						binaryReader.BaseStream.Position = position;
						result = this.locals;
					}
				}
			}
			return result;
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x00028EF0 File Offset: 0x000270F0
		public CodeBlockEntry[] GetCodeBlocks()
		{
			MonoSymbolFile symbolFile = this.SymbolFile;
			CodeBlockEntry[] result;
			lock (symbolFile)
			{
				bool flag = this.code_blocks != null;
				if (flag)
				{
					result = this.code_blocks;
				}
				else
				{
					bool flag2 = this.CodeBlockTableOffset == 0;
					if (flag2)
					{
						result = null;
					}
					else
					{
						MyBinaryReader binaryReader = this.SymbolFile.BinaryReader;
						long position = binaryReader.BaseStream.Position;
						binaryReader.BaseStream.Position = (long)this.CodeBlockTableOffset;
						int num = binaryReader.ReadLeb128();
						this.code_blocks = new CodeBlockEntry[num];
						for (int i = 0; i < num; i++)
						{
							this.code_blocks[i] = new CodeBlockEntry(i, binaryReader);
						}
						binaryReader.BaseStream.Position = position;
						result = this.code_blocks;
					}
				}
			}
			return result;
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x00028FD8 File Offset: 0x000271D8
		public ScopeVariable[] GetScopeVariables()
		{
			MonoSymbolFile symbolFile = this.SymbolFile;
			ScopeVariable[] result;
			lock (symbolFile)
			{
				bool flag = this.scope_vars != null;
				if (flag)
				{
					result = this.scope_vars;
				}
				else
				{
					bool flag2 = this.ScopeVariableTableOffset == 0;
					if (flag2)
					{
						result = null;
					}
					else
					{
						MyBinaryReader binaryReader = this.SymbolFile.BinaryReader;
						long position = binaryReader.BaseStream.Position;
						binaryReader.BaseStream.Position = (long)this.ScopeVariableTableOffset;
						int num = binaryReader.ReadLeb128();
						this.scope_vars = new ScopeVariable[num];
						for (int i = 0; i < num; i++)
						{
							this.scope_vars[i] = new ScopeVariable(binaryReader);
						}
						binaryReader.BaseStream.Position = position;
						result = this.scope_vars;
					}
				}
			}
			return result;
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x000290C0 File Offset: 0x000272C0
		public string GetRealName()
		{
			MonoSymbolFile symbolFile = this.SymbolFile;
			string result;
			lock (symbolFile)
			{
				bool flag = this.real_name != null;
				if (flag)
				{
					result = this.real_name;
				}
				else
				{
					bool flag2 = this.RealNameOffset == 0;
					if (flag2)
					{
						result = null;
					}
					else
					{
						this.real_name = this.SymbolFile.BinaryReader.ReadString(this.RealNameOffset);
						result = this.real_name;
					}
				}
			}
			return result;
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x00029144 File Offset: 0x00027344
		public int CompareTo(object obj)
		{
			MethodEntry methodEntry = (MethodEntry)obj;
			bool flag = methodEntry.Token < this.Token;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				bool flag2 = methodEntry.Token > this.Token;
				if (flag2)
				{
					result = -1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0002918C File Offset: 0x0002738C
		public override string ToString()
		{
			return string.Format("[Method {0}:{1:x}:{2}:{3}]", new object[]
			{
				this.index,
				this.Token,
				this.CompileUnitIndex,
				this.CompileUnit
			});
		}

		// Token: 0x040003D3 RID: 979
		public readonly int CompileUnitIndex;

		// Token: 0x040003D4 RID: 980
		public readonly int Token;

		// Token: 0x040003D5 RID: 981
		public readonly int NamespaceID;

		// Token: 0x040003D6 RID: 982
		private int DataOffset;

		// Token: 0x040003D7 RID: 983
		private int LocalVariableTableOffset;

		// Token: 0x040003D8 RID: 984
		private int LineNumberTableOffset;

		// Token: 0x040003D9 RID: 985
		private int CodeBlockTableOffset;

		// Token: 0x040003DA RID: 986
		private int ScopeVariableTableOffset;

		// Token: 0x040003DB RID: 987
		private int RealNameOffset;

		// Token: 0x040003DC RID: 988
		private MethodEntry.Flags flags;

		// Token: 0x040003DD RID: 989
		private int index;

		// Token: 0x040003DE RID: 990
		public readonly CompileUnitEntry CompileUnit;

		// Token: 0x040003DF RID: 991
		private LocalVariableEntry[] locals;

		// Token: 0x040003E0 RID: 992
		private CodeBlockEntry[] code_blocks;

		// Token: 0x040003E1 RID: 993
		private ScopeVariable[] scope_vars;

		// Token: 0x040003E2 RID: 994
		private LineNumberTable lnt;

		// Token: 0x040003E3 RID: 995
		private string real_name;

		// Token: 0x040003E4 RID: 996
		public readonly MonoSymbolFile SymbolFile;

		// Token: 0x040003E5 RID: 997
		public const int Size = 12;

		// Token: 0x020000BC RID: 188
		[Flags]
		public enum Flags
		{
			// Token: 0x040003E7 RID: 999
			LocalNamesAmbiguous = 1,
			// Token: 0x040003E8 RID: 1000
			ColumnsInfoIncluded = 2,
			// Token: 0x040003E9 RID: 1001
			EndInfoIncluded = 4
		}
	}
}
