using System;
using System.Collections.Generic;
using System.IO;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000BE RID: 190
	internal class MonoSymbolWriter
	{
		// Token: 0x06000669 RID: 1641 RVA: 0x00029314 File Offset: 0x00027514
		public MonoSymbolWriter(string filename)
		{
			this.methods = new List<SourceMethodBuilder>();
			this.sources = new List<SourceFileEntry>();
			this.comp_units = new List<CompileUnitEntry>();
			this.file = new MonoSymbolFile();
			this.filename = filename + ".mdb";
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x0600066A RID: 1642 RVA: 0x00029374 File Offset: 0x00027574
		public MonoSymbolFile SymbolFile
		{
			get
			{
				return this.file;
			}
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0002938C File Offset: 0x0002758C
		public void CloseNamespace()
		{
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x00029390 File Offset: 0x00027590
		public void DefineLocalVariable(int index, string name)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.AddLocal(index, name);
			}
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x000293BC File Offset: 0x000275BC
		public void DefineCapturedLocal(int scope_id, string name, string captured_name)
		{
			this.file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Local);
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x000293D0 File Offset: 0x000275D0
		public void DefineCapturedParameter(int scope_id, string name, string captured_name)
		{
			this.file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Parameter);
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x000293E4 File Offset: 0x000275E4
		public void DefineCapturedThis(int scope_id, string captured_name)
		{
			this.file.DefineCapturedVariable(scope_id, "this", captured_name, CapturedVariable.CapturedKind.This);
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x000293FC File Offset: 0x000275FC
		public void DefineCapturedScope(int scope_id, int id, string captured_name)
		{
			this.file.DefineCapturedScope(scope_id, id, captured_name);
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x00029410 File Offset: 0x00027610
		public void DefineScopeVariable(int scope, int index)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.AddScopeVariable(scope, index);
			}
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0002943C File Offset: 0x0002763C
		public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, bool is_hidden)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.MarkSequencePoint(offset, file, line, column, is_hidden);
			}
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0002946C File Offset: 0x0002766C
		public SourceMethodBuilder OpenMethod(ICompileUnit file, int ns_id, IMethodDef method)
		{
			SourceMethodBuilder result = new SourceMethodBuilder(file, ns_id, method);
			this.current_method_stack.Push(this.current_method);
			this.current_method = result;
			this.methods.Add(this.current_method);
			return result;
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x000294B4 File Offset: 0x000276B4
		public void CloseMethod()
		{
			this.current_method = this.current_method_stack.Pop();
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x000294C8 File Offset: 0x000276C8
		public SourceFileEntry DefineDocument(string url)
		{
			SourceFileEntry sourceFileEntry = new SourceFileEntry(this.file, url);
			this.sources.Add(sourceFileEntry);
			return sourceFileEntry;
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x000294F8 File Offset: 0x000276F8
		public SourceFileEntry DefineDocument(string url, byte[] guid, byte[] checksum)
		{
			SourceFileEntry sourceFileEntry = new SourceFileEntry(this.file, url, guid, checksum);
			this.sources.Add(sourceFileEntry);
			return sourceFileEntry;
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x00029528 File Offset: 0x00027728
		public CompileUnitEntry DefineCompilationUnit(SourceFileEntry source)
		{
			CompileUnitEntry compileUnitEntry = new CompileUnitEntry(this.file, source);
			this.comp_units.Add(compileUnitEntry);
			return compileUnitEntry;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x00029558 File Offset: 0x00027758
		public int DefineNamespace(string name, CompileUnitEntry unit, string[] using_clauses, int parent)
		{
			bool flag = unit == null || using_clauses == null;
			if (flag)
			{
				throw new NullReferenceException();
			}
			return unit.DefineNamespace(name, using_clauses, parent);
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x00029588 File Offset: 0x00027788
		public int OpenScope(int start_offset)
		{
			bool flag = this.current_method == null;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				this.current_method.StartBlock(CodeBlockEntry.Type.Lexical, start_offset);
				result = 0;
			}
			return result;
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x000295BC File Offset: 0x000277BC
		public void CloseScope(int end_offset)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.EndBlock(end_offset);
			}
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x000295E8 File Offset: 0x000277E8
		public void OpenCompilerGeneratedBlock(int start_offset)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.StartBlock(CodeBlockEntry.Type.CompilerGenerated, start_offset);
			}
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x00029614 File Offset: 0x00027814
		public void CloseCompilerGeneratedBlock(int end_offset)
		{
			bool flag = this.current_method == null;
			if (!flag)
			{
				this.current_method.EndBlock(end_offset);
			}
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x00029640 File Offset: 0x00027840
		public void StartIteratorBody(int start_offset)
		{
			this.current_method.StartBlock(CodeBlockEntry.Type.IteratorBody, start_offset);
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x00029654 File Offset: 0x00027854
		public void EndIteratorBody(int end_offset)
		{
			this.current_method.EndBlock(end_offset);
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x00029664 File Offset: 0x00027864
		public void StartIteratorDispatcher(int start_offset)
		{
			this.current_method.StartBlock(CodeBlockEntry.Type.IteratorDispatcher, start_offset);
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x00029678 File Offset: 0x00027878
		public void EndIteratorDispatcher(int end_offset)
		{
			this.current_method.EndBlock(end_offset);
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x00029688 File Offset: 0x00027888
		public void DefineAnonymousScope(int id)
		{
			this.file.DefineAnonymousScope(id);
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x00029698 File Offset: 0x00027898
		public void WriteSymbolFile(Guid guid)
		{
			foreach (SourceMethodBuilder sourceMethodBuilder in this.methods)
			{
				sourceMethodBuilder.DefineMethod(this.file);
			}
			try
			{
				File.Delete(this.filename);
			}
			catch
			{
			}
			using (FileStream fileStream = new FileStream(this.filename, FileMode.Create, FileAccess.Write))
			{
				this.file.CreateSymbolFile(guid, fileStream);
			}
		}

		// Token: 0x040003EE RID: 1006
		private List<SourceMethodBuilder> methods;

		// Token: 0x040003EF RID: 1007
		private List<SourceFileEntry> sources;

		// Token: 0x040003F0 RID: 1008
		private List<CompileUnitEntry> comp_units;

		// Token: 0x040003F1 RID: 1009
		protected readonly MonoSymbolFile file;

		// Token: 0x040003F2 RID: 1010
		private string filename;

		// Token: 0x040003F3 RID: 1011
		private SourceMethodBuilder current_method;

		// Token: 0x040003F4 RID: 1012
		private Stack<SourceMethodBuilder> current_method_stack = new Stack<SourceMethodBuilder>();
	}
}
