using System;
using System.IO;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000AC RID: 172
	internal class OffsetTable
	{
		// Token: 0x06000609 RID: 1545 RVA: 0x00026DC4 File Offset: 0x00024FC4
		internal OffsetTable()
		{
			int platform = (int)Environment.OSVersion.Platform;
			bool flag = platform != 4 && platform != 128;
			if (flag)
			{
				this.FileFlags |= OffsetTable.Flags.WindowsFileNames;
			}
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x00026E20 File Offset: 0x00025020
		internal OffsetTable(BinaryReader reader, int major_version, int minor_version)
		{
			this.TotalFileSize = reader.ReadInt32();
			this.DataSectionOffset = reader.ReadInt32();
			this.DataSectionSize = reader.ReadInt32();
			this.CompileUnitCount = reader.ReadInt32();
			this.CompileUnitTableOffset = reader.ReadInt32();
			this.CompileUnitTableSize = reader.ReadInt32();
			this.SourceCount = reader.ReadInt32();
			this.SourceTableOffset = reader.ReadInt32();
			this.SourceTableSize = reader.ReadInt32();
			this.MethodCount = reader.ReadInt32();
			this.MethodTableOffset = reader.ReadInt32();
			this.MethodTableSize = reader.ReadInt32();
			this.TypeCount = reader.ReadInt32();
			this.AnonymousScopeCount = reader.ReadInt32();
			this.AnonymousScopeTableOffset = reader.ReadInt32();
			this.AnonymousScopeTableSize = reader.ReadInt32();
			this.LineNumberTable_LineBase = reader.ReadInt32();
			this.LineNumberTable_LineRange = reader.ReadInt32();
			this.LineNumberTable_OpcodeBase = reader.ReadInt32();
			this.FileFlags = (OffsetTable.Flags)reader.ReadInt32();
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00026F3C File Offset: 0x0002513C
		internal void Write(BinaryWriter bw, int major_version, int minor_version)
		{
			bw.Write(this.TotalFileSize);
			bw.Write(this.DataSectionOffset);
			bw.Write(this.DataSectionSize);
			bw.Write(this.CompileUnitCount);
			bw.Write(this.CompileUnitTableOffset);
			bw.Write(this.CompileUnitTableSize);
			bw.Write(this.SourceCount);
			bw.Write(this.SourceTableOffset);
			bw.Write(this.SourceTableSize);
			bw.Write(this.MethodCount);
			bw.Write(this.MethodTableOffset);
			bw.Write(this.MethodTableSize);
			bw.Write(this.TypeCount);
			bw.Write(this.AnonymousScopeCount);
			bw.Write(this.AnonymousScopeTableOffset);
			bw.Write(this.AnonymousScopeTableSize);
			bw.Write(this.LineNumberTable_LineBase);
			bw.Write(this.LineNumberTable_LineRange);
			bw.Write(this.LineNumberTable_OpcodeBase);
			bw.Write((int)this.FileFlags);
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00027050 File Offset: 0x00025250
		public override string ToString()
		{
			return string.Format("OffsetTable [{0} - {1}:{2} - {3}:{4}:{5} - {6}:{7}:{8} - {9}]", new object[]
			{
				this.TotalFileSize,
				this.DataSectionOffset,
				this.DataSectionSize,
				this.SourceCount,
				this.SourceTableOffset,
				this.SourceTableSize,
				this.MethodCount,
				this.MethodTableOffset,
				this.MethodTableSize,
				this.TypeCount
			});
		}

		// Token: 0x04000374 RID: 884
		public const int MajorVersion = 50;

		// Token: 0x04000375 RID: 885
		public const int MinorVersion = 0;

		// Token: 0x04000376 RID: 886
		public const long Magic = 5037318119232611860L;

		// Token: 0x04000377 RID: 887
		public int TotalFileSize;

		// Token: 0x04000378 RID: 888
		public int DataSectionOffset;

		// Token: 0x04000379 RID: 889
		public int DataSectionSize;

		// Token: 0x0400037A RID: 890
		public int CompileUnitCount;

		// Token: 0x0400037B RID: 891
		public int CompileUnitTableOffset;

		// Token: 0x0400037C RID: 892
		public int CompileUnitTableSize;

		// Token: 0x0400037D RID: 893
		public int SourceCount;

		// Token: 0x0400037E RID: 894
		public int SourceTableOffset;

		// Token: 0x0400037F RID: 895
		public int SourceTableSize;

		// Token: 0x04000380 RID: 896
		public int MethodCount;

		// Token: 0x04000381 RID: 897
		public int MethodTableOffset;

		// Token: 0x04000382 RID: 898
		public int MethodTableSize;

		// Token: 0x04000383 RID: 899
		public int TypeCount;

		// Token: 0x04000384 RID: 900
		public int AnonymousScopeCount;

		// Token: 0x04000385 RID: 901
		public int AnonymousScopeTableOffset;

		// Token: 0x04000386 RID: 902
		public int AnonymousScopeTableSize;

		// Token: 0x04000387 RID: 903
		public OffsetTable.Flags FileFlags;

		// Token: 0x04000388 RID: 904
		public int LineNumberTable_LineBase = -1;

		// Token: 0x04000389 RID: 905
		public int LineNumberTable_LineRange = 8;

		// Token: 0x0400038A RID: 906
		public int LineNumberTable_OpcodeBase = 9;

		// Token: 0x020000AD RID: 173
		[Flags]
		public enum Flags
		{
			// Token: 0x0400038C RID: 908
			IsAspxSource = 1,
			// Token: 0x0400038D RID: 909
			WindowsFileNames = 2
		}
	}
}
