using System.Collections.Generic;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000BA RID: 186
	internal class LineNumberTable
	{
		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x00028028 File Offset: 0x00026228
		public LineNumberEntry[] LineNumbers
		{
			get
			{
				return this._line_numbers;
			}
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x00028040 File Offset: 0x00026240
		protected LineNumberTable(MonoSymbolFile file)
		{
			this.LineBase = file.OffsetTable.LineNumberTable_LineBase;
			this.LineRange = file.OffsetTable.LineNumberTable_LineRange;
			this.OpcodeBase = (byte)file.OffsetTable.LineNumberTable_OpcodeBase;
			this.MaxAddressIncrement = (int)(byte.MaxValue - this.OpcodeBase) / this.LineRange;
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x000280A4 File Offset: 0x000262A4
		internal LineNumberTable(MonoSymbolFile file, LineNumberEntry[] lines) : this(file)
		{
			this._line_numbers = lines;
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x000280B8 File Offset: 0x000262B8
		internal void Write(MonoSymbolFile file, MyBinaryWriter bw, bool hasColumnsInfo, bool hasEndInfo)
		{
			int num = (int)bw.BaseStream.Position;
			bool flag = false;
			int num2 = 1;
			int num3 = 0;
			int num4 = 1;
			for (int i = 0; i < this.LineNumbers.Length; i++)
			{
				int num5 = this.LineNumbers[i].Row - num2;
				int num6 = this.LineNumbers[i].Offset - num3;
				bool flag2 = this.LineNumbers[i].File != num4;
				if (flag2)
				{
					bw.Write(4);
					bw.WriteLeb128(this.LineNumbers[i].File);
					num4 = this.LineNumbers[i].File;
				}
				bool flag3 = this.LineNumbers[i].IsHidden != flag;
				if (flag3)
				{
					bw.Write(0);
					bw.Write(1);
					bw.Write(64);
					flag = this.LineNumbers[i].IsHidden;
				}
				bool flag4 = num6 >= this.MaxAddressIncrement;
				if (flag4)
				{
					bool flag5 = num6 < 2 * this.MaxAddressIncrement;
					if (flag5)
					{
						bw.Write(8);
						num6 -= this.MaxAddressIncrement;
					}
					else
					{
						bw.Write(2);
						bw.WriteLeb128(num6);
						num6 = 0;
					}
				}
				bool flag6 = num5 < this.LineBase || num5 >= this.LineBase + this.LineRange;
				if (flag6)
				{
					bw.Write(3);
					bw.WriteLeb128(num5);
					bool flag7 = num6 != 0;
					if (flag7)
					{
						bw.Write(2);
						bw.WriteLeb128(num6);
					}
					bw.Write(1);
				}
				else
				{
					byte value = (byte)(num5 - this.LineBase + this.LineRange * num6 + (int)this.OpcodeBase);
					bw.Write(value);
				}
				num2 = this.LineNumbers[i].Row;
				num3 = this.LineNumbers[i].Offset;
			}
			bw.Write(0);
			bw.Write(1);
			bw.Write(1);
			if (hasColumnsInfo)
			{
				for (int j = 0; j < this.LineNumbers.Length; j++)
				{
					LineNumberEntry lineNumberEntry = this.LineNumbers[j];
					bool flag8 = lineNumberEntry.Row >= 0;
					if (flag8)
					{
						bw.WriteLeb128(lineNumberEntry.Column);
					}
				}
			}
			if (hasEndInfo)
			{
				for (int k = 0; k < this.LineNumbers.Length; k++)
				{
					LineNumberEntry lineNumberEntry2 = this.LineNumbers[k];
					bool flag9 = lineNumberEntry2.EndRow == -1 || lineNumberEntry2.EndColumn == -1 || lineNumberEntry2.Row > lineNumberEntry2.EndRow;
					if (flag9)
					{
						bw.WriteLeb128(16777215);
					}
					else
					{
						bw.WriteLeb128(lineNumberEntry2.EndRow - lineNumberEntry2.Row);
						bw.WriteLeb128(lineNumberEntry2.EndColumn);
					}
				}
			}
			file.ExtendedLineNumberSize += (int)bw.BaseStream.Position - num;
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x000283D4 File Offset: 0x000265D4
		internal static LineNumberTable Read(MonoSymbolFile file, MyBinaryReader br, bool readColumnsInfo, bool readEndInfo)
		{
			LineNumberTable lineNumberTable = new LineNumberTable(file);
			lineNumberTable.DoRead(file, br, readColumnsInfo, readEndInfo);
			return lineNumberTable;
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x000283FC File Offset: 0x000265FC
		private void DoRead(MonoSymbolFile file, MyBinaryReader br, bool includesColumns, bool includesEnds)
		{
			List<LineNumberEntry> list = new List<LineNumberEntry>();
			bool flag = false;
			bool flag2 = false;
			int num = 1;
			int num2 = 0;
			int file2 = 1;
			byte b;
			for (;;)
			{
				b = br.ReadByte();
				bool flag3 = b == 0;
				if (flag3)
				{
					byte b2 = br.ReadByte();
					long position = br.BaseStream.Position + (long)((ulong)b2);
					b = br.ReadByte();
					bool flag4 = b == 1;
					if (flag4)
					{
						break;
					}
					bool flag5 = b == 64;
					if (flag5)
					{
						flag = !flag;
						flag2 = true;
					}
					else
					{
						bool flag6 = b >= 64 && b <= 127;
						if (!flag6)
						{
							goto IL_B0;
						}
					}
					br.BaseStream.Position = position;
				}
				else
				{
					bool flag7 = b < this.OpcodeBase;
					if (flag7)
					{
						switch (b)
						{
						case 1:
							list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
							flag2 = false;
							continue;
						case 2:
							num2 += br.ReadLeb128();
							flag2 = true;
							continue;
						case 3:
							num += br.ReadLeb128();
							flag2 = true;
							continue;
						case 4:
							file2 = br.ReadLeb128();
							flag2 = true;
							continue;
						case 8:
							num2 += this.MaxAddressIncrement;
							flag2 = true;
							continue;
						}
						goto Block_8;
					}
					b -= this.OpcodeBase;
					num2 += (int)b / this.LineRange;
					num += this.LineBase + (int)b % this.LineRange;
					list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
					flag2 = false;
				}
			}
			bool flag8 = flag2;
			if (flag8)
			{
				list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
			}
			this._line_numbers = list.ToArray();
			if (includesColumns)
			{
				for (int i = 0; i < this._line_numbers.Length; i++)
				{
					LineNumberEntry lineNumberEntry = this._line_numbers[i];
					bool flag9 = lineNumberEntry.Row >= 0;
					if (flag9)
					{
						lineNumberEntry.Column = br.ReadLeb128();
					}
				}
			}
			if (includesEnds)
			{
				for (int j = 0; j < this._line_numbers.Length; j++)
				{
					LineNumberEntry lineNumberEntry2 = this._line_numbers[j];
					int num3 = br.ReadLeb128();
					bool flag10 = num3 == 16777215;
					if (flag10)
					{
						lineNumberEntry2.EndRow = -1;
						lineNumberEntry2.EndColumn = -1;
					}
					else
					{
						lineNumberEntry2.EndRow = lineNumberEntry2.Row + num3;
						lineNumberEntry2.EndColumn = br.ReadLeb128();
					}
				}
			}
			return;
			IL_B0:
			throw new MonoSymbolFileException("Unknown extended opcode {0:x}", new object[]
			{
				b
			});
			Block_8:
			throw new MonoSymbolFileException("Unknown standard opcode {0:x} in LNT", new object[]
			{
				b
			});
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x000286C8 File Offset: 0x000268C8
		public bool GetMethodBounds(out LineNumberEntry start, out LineNumberEntry end)
		{
			bool flag = this._line_numbers.Length > 1;
			bool result;
			if (flag)
			{
				start = this._line_numbers[0];
				end = this._line_numbers[this._line_numbers.Length - 1];
				result = true;
			}
			else
			{
				start = LineNumberEntry.Null;
				end = LineNumberEntry.Null;
				result = false;
			}
			return result;
		}

		// Token: 0x040003C2 RID: 962
		protected LineNumberEntry[] _line_numbers;

		// Token: 0x040003C3 RID: 963
		public readonly int LineBase;

		// Token: 0x040003C4 RID: 964
		public readonly int LineRange;

		// Token: 0x040003C5 RID: 965
		public readonly byte OpcodeBase;

		// Token: 0x040003C6 RID: 966
		public readonly int MaxAddressIncrement;

		// Token: 0x040003C7 RID: 967
		public const int Default_LineBase = -1;

		// Token: 0x040003C8 RID: 968
		public const int Default_LineRange = 8;

		// Token: 0x040003C9 RID: 969
		public const byte Default_OpcodeBase = 9;

		// Token: 0x040003CA RID: 970
		public const byte DW_LNS_copy = 1;

		// Token: 0x040003CB RID: 971
		public const byte DW_LNS_advance_pc = 2;

		// Token: 0x040003CC RID: 972
		public const byte DW_LNS_advance_line = 3;

		// Token: 0x040003CD RID: 973
		public const byte DW_LNS_set_file = 4;

		// Token: 0x040003CE RID: 974
		public const byte DW_LNS_const_add_pc = 8;

		// Token: 0x040003CF RID: 975
		public const byte DW_LNE_end_sequence = 1;

		// Token: 0x040003D0 RID: 976
		public const byte DW_LNE_MONO_negate_is_hidden = 64;

		// Token: 0x040003D1 RID: 977
		internal const byte DW_LNE_MONO__extensions_start = 64;

		// Token: 0x040003D2 RID: 978
		internal const byte DW_LNE_MONO__extensions_end = 127;
	}
}
