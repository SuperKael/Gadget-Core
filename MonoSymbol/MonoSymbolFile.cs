using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000AB RID: 171
	internal class MonoSymbolFile : IDisposable
	{
		// Token: 0x060005E2 RID: 1506 RVA: 0x00025CCC File Offset: 0x00023ECC
		public MonoSymbolFile()
		{
			this.ot = new OffsetTable();
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x00025D40 File Offset: 0x00023F40
		public int AddSource(SourceFileEntry source)
		{
			this.sources.Add(source);
			return this.sources.Count;
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x00025D6C File Offset: 0x00023F6C
		public int AddCompileUnit(CompileUnitEntry entry)
		{
			this.comp_units.Add(entry);
			return this.comp_units.Count;
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x00025D98 File Offset: 0x00023F98
		public void AddMethod(MethodEntry entry)
		{
			this.methods.Add(entry);
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x00025DA8 File Offset: 0x00023FA8
		public MethodEntry DefineMethod(CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, MethodEntry.Flags flags, int namespace_id)
		{
			bool flag = this.reader != null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			MethodEntry methodEntry = new MethodEntry(this, comp_unit, token, scope_vars, locals, lines, code_blocks, real_name, flags, namespace_id);
			this.AddMethod(methodEntry);
			return methodEntry;
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00025DEC File Offset: 0x00023FEC
		internal void DefineAnonymousScope(int id)
		{
			bool flag = this.reader != null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			bool flag2 = this.anonymous_scopes == null;
			if (flag2)
			{
				this.anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
			}
			this.anonymous_scopes.Add(id, new AnonymousScopeEntry(id));
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x00025E38 File Offset: 0x00024038
		internal void DefineCapturedVariable(int scope_id, string name, string captured_name, CapturedVariable.CapturedKind kind)
		{
			bool flag = this.reader != null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			AnonymousScopeEntry anonymousScopeEntry = this.anonymous_scopes[scope_id];
			anonymousScopeEntry.AddCapturedVariable(name, captured_name, kind);
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x00025E74 File Offset: 0x00024074
		internal void DefineCapturedScope(int scope_id, int id, string captured_name)
		{
			bool flag = this.reader != null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			AnonymousScopeEntry anonymousScopeEntry = this.anonymous_scopes[scope_id];
			anonymousScopeEntry.AddCapturedScope(id, captured_name);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00025EAC File Offset: 0x000240AC
		internal int GetNextTypeIndex()
		{
			int result = this.last_type_index + 1;
			this.last_type_index = result;
			return result;
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00025ED0 File Offset: 0x000240D0
		internal int GetNextMethodIndex()
		{
			int result = this.last_method_index + 1;
			this.last_method_index = result;
			return result;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00025EF4 File Offset: 0x000240F4
		internal int GetNextNamespaceIndex()
		{
			int result = this.last_namespace_index + 1;
			this.last_namespace_index = result;
			return result;
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00025F18 File Offset: 0x00024118
		private void Write(MyBinaryWriter bw, Guid guid)
		{
			bw.Write(5037318119232611860L);
			bw.Write(this.MajorVersion);
			bw.Write(this.MinorVersion);
			bw.Write(guid.ToByteArray());
			long position = bw.BaseStream.Position;
			this.ot.Write(bw, this.MajorVersion, this.MinorVersion);
			this.methods.Sort();
			for (int i = 0; i < this.methods.Count; i++)
			{
				this.methods[i].Index = i + 1;
			}
			this.ot.DataSectionOffset = (int)bw.BaseStream.Position;
			foreach (SourceFileEntry sourceFileEntry in this.sources)
			{
				sourceFileEntry.WriteData(bw);
			}
			foreach (CompileUnitEntry compileUnitEntry in this.comp_units)
			{
				compileUnitEntry.WriteData(bw);
			}
			foreach (MethodEntry methodEntry in this.methods)
			{
				methodEntry.WriteData(this, bw);
			}
			this.ot.DataSectionSize = (int)bw.BaseStream.Position - this.ot.DataSectionOffset;
			this.ot.MethodTableOffset = (int)bw.BaseStream.Position;
			for (int j = 0; j < this.methods.Count; j++)
			{
				MethodEntry methodEntry2 = this.methods[j];
				methodEntry2.Write(bw);
			}
			this.ot.MethodTableSize = (int)bw.BaseStream.Position - this.ot.MethodTableOffset;
			this.ot.SourceTableOffset = (int)bw.BaseStream.Position;
			for (int k = 0; k < this.sources.Count; k++)
			{
				SourceFileEntry sourceFileEntry2 = this.sources[k];
				sourceFileEntry2.Write(bw);
			}
			this.ot.SourceTableSize = (int)bw.BaseStream.Position - this.ot.SourceTableOffset;
			this.ot.CompileUnitTableOffset = (int)bw.BaseStream.Position;
			for (int l = 0; l < this.comp_units.Count; l++)
			{
				CompileUnitEntry compileUnitEntry2 = this.comp_units[l];
				compileUnitEntry2.Write(bw);
			}
			this.ot.CompileUnitTableSize = (int)bw.BaseStream.Position - this.ot.CompileUnitTableOffset;
			this.ot.AnonymousScopeCount = ((this.anonymous_scopes != null) ? this.anonymous_scopes.Count : 0);
			this.ot.AnonymousScopeTableOffset = (int)bw.BaseStream.Position;
			bool flag = this.anonymous_scopes != null;
			if (flag)
			{
				foreach (AnonymousScopeEntry anonymousScopeEntry in this.anonymous_scopes.Values)
				{
					anonymousScopeEntry.Write(bw);
				}
			}
			this.ot.AnonymousScopeTableSize = (int)bw.BaseStream.Position - this.ot.AnonymousScopeTableOffset;
			this.ot.TypeCount = this.last_type_index;
			this.ot.MethodCount = this.methods.Count;
			this.ot.SourceCount = this.sources.Count;
			this.ot.CompileUnitCount = this.comp_units.Count;
			this.ot.TotalFileSize = (int)bw.BaseStream.Position;
			bw.Seek((int)position, SeekOrigin.Begin);
			this.ot.Write(bw, this.MajorVersion, this.MinorVersion);
			bw.Seek(0, SeekOrigin.End);
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00026380 File Offset: 0x00024580
		public void CreateSymbolFile(Guid guid, FileStream fs)
		{
			bool flag = this.reader != null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			this.Write(new MyBinaryWriter(fs), guid);
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x000263B0 File Offset: 0x000245B0
		private MonoSymbolFile(Stream stream)
		{
			this.reader = new MyBinaryReader(stream);
			try
			{
				long num = this.reader.ReadInt64();
				int num2 = this.reader.ReadInt32();
				int num3 = this.reader.ReadInt32();
				bool flag = num != 5037318119232611860L;
				if (flag)
				{
					throw new MonoSymbolFileException("Symbol file is not a valid", new object[0]);
				}
				bool flag2 = num2 != 50;
				if (flag2)
				{
					throw new MonoSymbolFileException("Symbol file has version {0} but expected {1}", new object[]
					{
						num2,
						50
					});
				}
				bool flag3 = num3 != 0;
				if (flag3)
				{
					throw new MonoSymbolFileException("Symbol file has version {0}.{1} but expected {2}.{3}", new object[]
					{
						num2,
						num3,
						50,
						0
					});
				}
				this.MajorVersion = num2;
				this.MinorVersion = num3;
				this.guid = new Guid(this.reader.ReadBytes(16));
				this.ot = new OffsetTable(this.reader, num2, num3);
			}
			catch (Exception innerException)
			{
				throw new MonoSymbolFileException("Cannot read symbol file", innerException);
			}
			this.source_file_hash = new Dictionary<int, SourceFileEntry>();
			this.compile_unit_hash = new Dictionary<int, CompileUnitEntry>();
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00026554 File Offset: 0x00024754
		public static MonoSymbolFile ReadSymbolFile(Assembly assembly)
		{
			string location = assembly.Location;
			string mdbFilename = location + ".mdb";
			Module[] modules = assembly.GetModules();
			Guid moduleVersionId = modules[0].ModuleVersionId;
			return MonoSymbolFile.ReadSymbolFile(mdbFilename, moduleVersionId);
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00026594 File Offset: 0x00024794
		public static MonoSymbolFile ReadSymbolFile(string mdbFilename)
		{
			return MonoSymbolFile.ReadSymbolFile(new FileStream(mdbFilename, FileMode.Open, FileAccess.Read));
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x000265B4 File Offset: 0x000247B4
		public static MonoSymbolFile ReadSymbolFile(string mdbFilename, Guid assemblyGuid)
		{
			MonoSymbolFile monoSymbolFile = MonoSymbolFile.ReadSymbolFile(mdbFilename);
			bool flag = assemblyGuid != monoSymbolFile.guid;
			if (flag)
			{
				throw new MonoSymbolFileException("Symbol file `{0}' does not match assembly", new object[]
				{
					mdbFilename
				});
			}
			return monoSymbolFile;
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x000265F4 File Offset: 0x000247F4
		public static MonoSymbolFile ReadSymbolFile(Stream stream)
		{
			return new MonoSymbolFile(stream);
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x060005F4 RID: 1524 RVA: 0x0002660C File Offset: 0x0002480C
		public int CompileUnitCount
		{
			get
			{
				return this.ot.CompileUnitCount;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x060005F5 RID: 1525 RVA: 0x0002662C File Offset: 0x0002482C
		public int SourceCount
		{
			get
			{
				return this.ot.SourceCount;
			}
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x060005F6 RID: 1526 RVA: 0x0002664C File Offset: 0x0002484C
		public int MethodCount
		{
			get
			{
				return this.ot.MethodCount;
			}
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x060005F7 RID: 1527 RVA: 0x0002666C File Offset: 0x0002486C
		public int TypeCount
		{
			get
			{
				return this.ot.TypeCount;
			}
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x0002668C File Offset: 0x0002488C
		public int AnonymousScopeCount
		{
			get
			{
				return this.ot.AnonymousScopeCount;
			}
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x060005F9 RID: 1529 RVA: 0x000266AC File Offset: 0x000248AC
		public int NamespaceCount
		{
			get
			{
				return this.last_namespace_index;
			}
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x000266C4 File Offset: 0x000248C4
		public Guid Guid
		{
			get
			{
				return this.guid;
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x060005FB RID: 1531 RVA: 0x000266DC File Offset: 0x000248DC
		public OffsetTable OffsetTable
		{
			get
			{
				return this.ot;
			}
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x000266F4 File Offset: 0x000248F4
		public SourceFileEntry GetSourceFile(int index)
		{
			bool flag = index < 1 || index > this.ot.SourceCount;
			if (flag)
			{
				throw new ArgumentException();
			}
			bool flag2 = this.reader == null;
			if (flag2)
			{
				throw new InvalidOperationException();
			}
			SourceFileEntry result;
			lock (this)
			{
				SourceFileEntry sourceFileEntry;
				bool flag3 = this.source_file_hash.TryGetValue(index, out sourceFileEntry);
				if (flag3)
				{
					result = sourceFileEntry;
				}
				else
				{
					long position = this.reader.BaseStream.Position;
					this.reader.BaseStream.Position = (long)(this.ot.SourceTableOffset + SourceFileEntry.Size * (index - 1));
					sourceFileEntry = new SourceFileEntry(this, this.reader);
					this.source_file_hash.Add(index, sourceFileEntry);
					this.reader.BaseStream.Position = position;
					result = sourceFileEntry;
				}
			}
			return result;
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x000267E0 File Offset: 0x000249E0
		public SourceFileEntry[] Sources
		{
			get
			{
				bool flag = this.reader == null;
				if (flag)
				{
					throw new InvalidOperationException();
				}
				SourceFileEntry[] array = new SourceFileEntry[this.SourceCount];
				for (int i = 0; i < this.SourceCount; i++)
				{
					array[i] = this.GetSourceFile(i + 1);
				}
				return array;
			}
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x00026838 File Offset: 0x00024A38
		public CompileUnitEntry GetCompileUnit(int index)
		{
			bool flag = index < 1 || index > this.ot.CompileUnitCount;
			if (flag)
			{
				throw new ArgumentException();
			}
			bool flag2 = this.reader == null;
			if (flag2)
			{
				throw new InvalidOperationException();
			}
			CompileUnitEntry result;
			lock (this)
			{
				CompileUnitEntry compileUnitEntry;
				bool flag3 = this.compile_unit_hash.TryGetValue(index, out compileUnitEntry);
				if (flag3)
				{
					result = compileUnitEntry;
				}
				else
				{
					long position = this.reader.BaseStream.Position;
					this.reader.BaseStream.Position = (long)(this.ot.CompileUnitTableOffset + CompileUnitEntry.Size * (index - 1));
					compileUnitEntry = new CompileUnitEntry(this, this.reader);
					this.compile_unit_hash.Add(index, compileUnitEntry);
					this.reader.BaseStream.Position = position;
					result = compileUnitEntry;
				}
			}
			return result;
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x00026924 File Offset: 0x00024B24
		public CompileUnitEntry[] CompileUnits
		{
			get
			{
				bool flag = this.reader == null;
				if (flag)
				{
					throw new InvalidOperationException();
				}
				CompileUnitEntry[] array = new CompileUnitEntry[this.CompileUnitCount];
				for (int i = 0; i < this.CompileUnitCount; i++)
				{
					array[i] = this.GetCompileUnit(i + 1);
				}
				return array;
			}
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x0002697C File Offset: 0x00024B7C
		private void read_methods()
		{
			lock (this)
			{
				bool flag = this.method_token_hash != null;
				if (!flag)
				{
					this.method_token_hash = new Dictionary<int, MethodEntry>();
					this.method_list = new List<MethodEntry>();
					long position = this.reader.BaseStream.Position;
					this.reader.BaseStream.Position = (long)this.ot.MethodTableOffset;
					for (int i = 0; i < this.MethodCount; i++)
					{
						MethodEntry methodEntry = new MethodEntry(this, this.reader, i + 1);
						this.method_token_hash.Add(methodEntry.Token, methodEntry);
						this.method_list.Add(methodEntry);
					}
					this.reader.BaseStream.Position = position;
				}
			}
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00026A64 File Offset: 0x00024C64
		public MethodEntry GetMethodByToken(int token)
		{
			bool flag = this.reader == null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			MethodEntry result;
			lock (this)
			{
				this.read_methods();
				MethodEntry methodEntry;
				this.method_token_hash.TryGetValue(token, out methodEntry);
				result = methodEntry;
			}
			return result;
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00026AC4 File Offset: 0x00024CC4
		public MethodEntry GetMethod(int index)
		{
			bool flag = index < 1 || index > this.ot.MethodCount;
			if (flag)
			{
				throw new ArgumentException();
			}
			bool flag2 = this.reader == null;
			if (flag2)
			{
				throw new InvalidOperationException();
			}
			MethodEntry result;
			lock (this)
			{
				this.read_methods();
				result = this.method_list[index - 1];
			}
			return result;
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000603 RID: 1539 RVA: 0x00026B40 File Offset: 0x00024D40
		public MethodEntry[] Methods
		{
			get
			{
				bool flag = this.reader == null;
				if (flag)
				{
					throw new InvalidOperationException();
				}
				MethodEntry[] result;
				lock (this)
				{
					this.read_methods();
					MethodEntry[] array = new MethodEntry[this.MethodCount];
					this.method_list.CopyTo(array, 0);
					result = array;
				}
				return result;
			}
		}

		// Token: 0x06000604 RID: 1540 RVA: 0x00026BAC File Offset: 0x00024DAC
		public int FindSource(string file_name)
		{
			bool flag = this.reader == null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			int result;
			lock (this)
			{
				bool flag2 = this.source_name_hash == null;
				if (flag2)
				{
					this.source_name_hash = new Dictionary<string, int>();
					for (int i = 0; i < this.ot.SourceCount; i++)
					{
						SourceFileEntry sourceFile = this.GetSourceFile(i + 1);
						this.source_name_hash.Add(sourceFile.FileName, i);
					}
				}
				int num;
				bool flag3 = !this.source_name_hash.TryGetValue(file_name, out num);
				if (flag3)
				{
					result = -1;
				}
				else
				{
					result = num;
				}
			}
			return result;
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00026C70 File Offset: 0x00024E70
		public AnonymousScopeEntry GetAnonymousScope(int id)
		{
			bool flag = this.reader == null;
			if (flag)
			{
				throw new InvalidOperationException();
			}
			AnonymousScopeEntry result;
			lock (this)
			{
				bool flag2 = this.anonymous_scopes != null;
				if (flag2)
				{
					AnonymousScopeEntry anonymousScopeEntry;
					this.anonymous_scopes.TryGetValue(id, out anonymousScopeEntry);
					result = anonymousScopeEntry;
				}
				else
				{
					this.anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
					this.reader.BaseStream.Position = (long)this.ot.AnonymousScopeTableOffset;
					for (int i = 0; i < this.ot.AnonymousScopeCount; i++)
					{
						AnonymousScopeEntry anonymousScopeEntry = new AnonymousScopeEntry(this.reader);
						this.anonymous_scopes.Add(anonymousScopeEntry.ID, anonymousScopeEntry);
					}
					result = this.anonymous_scopes[id];
				}
			}
			return result;
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000606 RID: 1542 RVA: 0x00026D54 File Offset: 0x00024F54
		internal MyBinaryReader BinaryReader
		{
			get
			{
				bool flag = this.reader == null;
				if (flag)
				{
					throw new InvalidOperationException();
				}
				return this.reader;
			}
		}

		// Token: 0x06000607 RID: 1543 RVA: 0x00026D80 File Offset: 0x00024F80
		public void Dispose()
		{
			this.Dispose(true);
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x00026D8C File Offset: 0x00024F8C
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				bool flag = this.reader != null;
				if (flag)
				{
					this.reader.Close();
					this.reader = null;
				}
			}
		}

		// Token: 0x0400035D RID: 861
		private List<MethodEntry> methods = new List<MethodEntry>();

		// Token: 0x0400035E RID: 862
		private List<SourceFileEntry> sources = new List<SourceFileEntry>();

		// Token: 0x0400035F RID: 863
		private List<CompileUnitEntry> comp_units = new List<CompileUnitEntry>();

		// Token: 0x04000360 RID: 864
		private Dictionary<int, AnonymousScopeEntry> anonymous_scopes;

		// Token: 0x04000361 RID: 865
		private OffsetTable ot;

		// Token: 0x04000362 RID: 866
		private int last_type_index;

		// Token: 0x04000363 RID: 867
		private int last_method_index;

		// Token: 0x04000364 RID: 868
		private int last_namespace_index;

		// Token: 0x04000365 RID: 869
		public readonly int MajorVersion = 50;

		// Token: 0x04000366 RID: 870
		public readonly int MinorVersion = 0;

		// Token: 0x04000367 RID: 871
		public int NumLineNumbers;

		// Token: 0x04000368 RID: 872
		private MyBinaryReader reader;

		// Token: 0x04000369 RID: 873
		private Dictionary<int, SourceFileEntry> source_file_hash;

		// Token: 0x0400036A RID: 874
		private Dictionary<int, CompileUnitEntry> compile_unit_hash;

		// Token: 0x0400036B RID: 875
		private List<MethodEntry> method_list;

		// Token: 0x0400036C RID: 876
		private Dictionary<int, MethodEntry> method_token_hash;

		// Token: 0x0400036D RID: 877
		private Dictionary<string, int> source_name_hash;

		// Token: 0x0400036E RID: 878
		private Guid guid;

		// Token: 0x0400036F RID: 879
		internal int LineNumberCount = 0;

		// Token: 0x04000370 RID: 880
		internal int LocalCount = 0;

		// Token: 0x04000371 RID: 881
		internal int StringSize = 0;

		// Token: 0x04000372 RID: 882
		internal int LineNumberSize = 0;

		// Token: 0x04000373 RID: 883
		internal int ExtendedLineNumberSize = 0;
	}
}
