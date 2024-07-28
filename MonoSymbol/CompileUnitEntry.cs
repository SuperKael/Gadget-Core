using System;
using System.Collections.Generic;
using System.IO;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B8 RID: 184
	internal class CompileUnitEntry : ICompileUnit
	{
		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x00027840 File Offset: 0x00025A40
		public static int Size
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000634 RID: 1588 RVA: 0x00027854 File Offset: 0x00025A54
		CompileUnitEntry ICompileUnit.Entry
		{
			get
			{
				return this;
			}
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x00027868 File Offset: 0x00025A68
		public CompileUnitEntry(MonoSymbolFile file, SourceFileEntry source)
		{
			this.file = file;
			this.source = source;
			this.Index = file.AddCompileUnit(this);
			this.creating = true;
			this.namespaces = new List<NamespaceEntry>();
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x000278A0 File Offset: 0x00025AA0
		public void AddFile(SourceFileEntry file)
		{
			bool flag = !this.creating;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			bool flag2 = this.include_files == null;
			if (flag2)
			{
				this.include_files = new List<SourceFileEntry>();
			}
			this.include_files.Add(file);
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000637 RID: 1591 RVA: 0x000278E8 File Offset: 0x00025AE8
		public SourceFileEntry SourceFile
		{
			get
			{
				bool flag = this.creating;
				SourceFileEntry result;
				if (flag)
				{
					result = this.source;
				}
				else
				{
					this.ReadData();
					result = this.source;
				}
				return result;
			}
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0002791C File Offset: 0x00025B1C
		public int DefineNamespace(string name, string[] using_clauses, int parent)
		{
			bool flag = !this.creating;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			int nextNamespaceIndex = this.file.GetNextNamespaceIndex();
			NamespaceEntry item = new NamespaceEntry(name, nextNamespaceIndex, using_clauses, parent);
			this.namespaces.Add(item);
			return nextNamespaceIndex;
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x00027968 File Offset: 0x00025B68
		internal void WriteData(MyBinaryWriter bw)
		{
			this.DataOffset = (int)bw.BaseStream.Position;
			bw.WriteLeb128(this.source.Index);
			int value = (this.include_files != null) ? this.include_files.Count : 0;
			bw.WriteLeb128(value);
			bool flag = this.include_files != null;
			if (flag)
			{
				foreach (SourceFileEntry sourceFileEntry in this.include_files)
				{
					bw.WriteLeb128(sourceFileEntry.Index);
				}
			}
			bw.WriteLeb128(this.namespaces.Count);
			foreach (NamespaceEntry namespaceEntry in this.namespaces)
			{
				namespaceEntry.Write(this.file, bw);
			}
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x00027A78 File Offset: 0x00025C78
		internal void Write(BinaryWriter bw)
		{
			bw.Write(this.Index);
			bw.Write(this.DataOffset);
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x00027A98 File Offset: 0x00025C98
		internal CompileUnitEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.file = file;
			this.Index = reader.ReadInt32();
			this.DataOffset = reader.ReadInt32();
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x00027AC4 File Offset: 0x00025CC4
		public void ReadAll()
		{
			this.ReadData();
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x00027AD0 File Offset: 0x00025CD0
		private void ReadData()
		{
			bool flag = this.creating;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			MonoSymbolFile obj = this.file;
			lock (obj)
			{
				bool flag2 = this.namespaces != null;
				if (!flag2)
				{
					MyBinaryReader binaryReader = this.file.BinaryReader;
					int num = (int)binaryReader.BaseStream.Position;
					binaryReader.BaseStream.Position = (long)this.DataOffset;
					int index = binaryReader.ReadLeb128();
					this.source = this.file.GetSourceFile(index);
					int num2 = binaryReader.ReadLeb128();
					bool flag3 = num2 > 0;
					if (flag3)
					{
						this.include_files = new List<SourceFileEntry>();
						for (int i = 0; i < num2; i++)
						{
							this.include_files.Add(this.file.GetSourceFile(binaryReader.ReadLeb128()));
						}
					}
					int num3 = binaryReader.ReadLeb128();
					this.namespaces = new List<NamespaceEntry>();
					for (int j = 0; j < num3; j++)
					{
						this.namespaces.Add(new NamespaceEntry(this.file, binaryReader));
					}
					binaryReader.BaseStream.Position = (long)num;
				}
			}
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x0600063E RID: 1598 RVA: 0x00027C24 File Offset: 0x00025E24
		public NamespaceEntry[] Namespaces
		{
			get
			{
				this.ReadData();
				NamespaceEntry[] array = new NamespaceEntry[this.namespaces.Count];
				this.namespaces.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x0600063F RID: 1599 RVA: 0x00027C60 File Offset: 0x00025E60
		public SourceFileEntry[] IncludeFiles
		{
			get
			{
				this.ReadData();
				bool flag = this.include_files == null;
				SourceFileEntry[] result;
				if (flag)
				{
					result = new SourceFileEntry[0];
				}
				else
				{
					SourceFileEntry[] array = new SourceFileEntry[this.include_files.Count];
					this.include_files.CopyTo(array, 0);
					result = array;
				}
				return result;
			}
		}

		// Token: 0x040003B2 RID: 946
		public readonly int Index;

		// Token: 0x040003B3 RID: 947
		private int DataOffset;

		// Token: 0x040003B4 RID: 948
		private MonoSymbolFile file;

		// Token: 0x040003B5 RID: 949
		private SourceFileEntry source;

		// Token: 0x040003B6 RID: 950
		private List<SourceFileEntry> include_files;

		// Token: 0x040003B7 RID: 951
		private List<NamespaceEntry> namespaces;

		// Token: 0x040003B8 RID: 952
		private bool creating;
	}
}
