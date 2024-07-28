using System;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000C0 RID: 192
	internal class SymbolWriterImpl : ISymbolWriter
	{
		// Token: 0x06000693 RID: 1683 RVA: 0x00029C04 File Offset: 0x00027E04
		public SymbolWriterImpl(ModuleBuilder mb)
		{
			this.mb = mb;
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00029C2C File Offset: 0x00027E2C
		public void Close()
		{
			MethodInfo method = typeof(ModuleBuilder).GetMethod("Mono_GetGuid", BindingFlags.Static | BindingFlags.NonPublic);
			bool flag = method == null;
			if (!flag)
			{
				this.get_guid_func = (SymbolWriterImpl.GetGuidFunc)Delegate.CreateDelegate(typeof(SymbolWriterImpl.GetGuidFunc), method);
				this.msw.WriteSymbolFile(this.get_guid_func(this.mb));
			}
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x00029C94 File Offset: 0x00027E94
		public void CloseMethod()
		{
			bool flag = this.methodOpened;
			if (flag)
			{
				this.methodOpened = false;
				this.nextLocalIndex = 0;
				this.msw.CloseMethod();
			}
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x00029CC8 File Offset: 0x00027EC8
		public void CloseNamespace()
		{
			this.namespaceStack.Pop();
			this.msw.CloseNamespace();
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00029CE4 File Offset: 0x00027EE4
		public void CloseScope(int endOffset)
		{
			this.msw.CloseScope(endOffset);
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00029CF4 File Offset: 0x00027EF4
		public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			SymbolDocumentWriterImpl symbolDocumentWriterImpl = (SymbolDocumentWriterImpl)this.documents[url];
			bool flag = symbolDocumentWriterImpl == null;
			if (flag)
			{
				SourceFileEntry source = this.msw.DefineDocument(url);
				CompileUnitEntry comp_unit = this.msw.DefineCompilationUnit(source);
				symbolDocumentWriterImpl = new SymbolDocumentWriterImpl(comp_unit);
				this.documents[url] = symbolDocumentWriterImpl;
			}
			return symbolDocumentWriterImpl;
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00029D54 File Offset: 0x00027F54
		public void DefineField(SymbolToken parent, string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x00029D58 File Offset: 0x00027F58
		public void DefineGlobalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x00029D5C File Offset: 0x00027F5C
		public void DefineLocalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
		{
			MonoSymbolWriter monoSymbolWriter = this.msw;
			int num = this.nextLocalIndex;
			this.nextLocalIndex = num + 1;
			monoSymbolWriter.DefineLocalVariable(num, name);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00029D88 File Offset: 0x00027F88
		public void DefineParameter(string name, ParameterAttributes attributes, int sequence, SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x00029D8C File Offset: 0x00027F8C
		public void DefineSequencePoints(ISymbolDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
		{
			SymbolDocumentWriterImpl symbolDocumentWriterImpl = (SymbolDocumentWriterImpl)document;
			SourceFileEntry file = (symbolDocumentWriterImpl != null) ? symbolDocumentWriterImpl.Entry.SourceFile : null;
			for (int i = 0; i < offsets.Length; i++)
			{
				bool flag = i > 0 && offsets[i] == offsets[i - 1] && lines[i] == lines[i - 1] && columns[i] == columns[i - 1];
				if (!flag)
				{
					this.msw.MarkSequencePoint(offsets[i], file, lines[i], columns[i], false);
				}
			}
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x00029E10 File Offset: 0x00028010
		public void Initialize(IntPtr emitter, string filename, bool fFullBuild)
		{
			this.msw = new MonoSymbolWriter(filename);
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x00029E20 File Offset: 0x00028020
		public void OpenMethod(SymbolToken method)
		{
			this.currentToken = method.GetToken();
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x00029E30 File Offset: 0x00028030
		public void OpenNamespace(string name)
		{
			NamespaceInfo namespaceInfo = new NamespaceInfo();
			namespaceInfo.NamespaceID = -1;
			namespaceInfo.Name = name;
			this.namespaceStack.Push(namespaceInfo);
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x00029E60 File Offset: 0x00028060
		public int OpenScope(int startOffset)
		{
			return this.msw.OpenScope(startOffset);
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x00029E80 File Offset: 0x00028080
		public void SetMethodSourceRange(ISymbolDocumentWriter startDoc, int startLine, int startColumn, ISymbolDocumentWriter endDoc, int endLine, int endColumn)
		{
			int currentNamespace = this.GetCurrentNamespace(startDoc);
			SourceMethodImpl method = new SourceMethodImpl(this.methodName, this.currentToken, currentNamespace);
			this.msw.OpenMethod(((ICompileUnit)startDoc).Entry, currentNamespace, method);
			this.methodOpened = true;
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x00029ECC File Offset: 0x000280CC
		public void SetScopeRange(int scopeID, int startOffset, int endOffset)
		{
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x00029ED0 File Offset: 0x000280D0
		public void SetSymAttribute(SymbolToken parent, string name, byte[] data)
		{
			bool flag = name == "__name";
			if (flag)
			{
				this.methodName = Encoding.UTF8.GetString(data);
			}
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x00029F00 File Offset: 0x00028100
		public void SetUnderlyingWriter(IntPtr underlyingWriter)
		{
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00029F04 File Offset: 0x00028104
		public void SetUserEntryPoint(SymbolToken entryMethod)
		{
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x00029F08 File Offset: 0x00028108
		public void UsingNamespace(string fullName)
		{
			bool flag = this.namespaceStack.Count == 0;
			if (flag)
			{
				this.OpenNamespace("");
			}
			NamespaceInfo namespaceInfo = (NamespaceInfo)this.namespaceStack.Peek();
			bool flag2 = namespaceInfo.NamespaceID != -1;
			if (flag2)
			{
				NamespaceInfo namespaceInfo2 = namespaceInfo;
				this.CloseNamespace();
				this.OpenNamespace(namespaceInfo2.Name);
				namespaceInfo = (NamespaceInfo)this.namespaceStack.Peek();
				namespaceInfo.UsingClauses = namespaceInfo2.UsingClauses;
			}
			namespaceInfo.UsingClauses.Add(fullName);
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x00029F9C File Offset: 0x0002819C
		private int GetCurrentNamespace(ISymbolDocumentWriter doc)
		{
			bool flag = this.namespaceStack.Count == 0;
			if (flag)
			{
				this.OpenNamespace("");
			}
			NamespaceInfo namespaceInfo = (NamespaceInfo)this.namespaceStack.Peek();
			bool flag2 = namespaceInfo.NamespaceID == -1;
			if (flag2)
			{
				string[] using_clauses = (string[])namespaceInfo.UsingClauses.ToArray(typeof(string));
				int parent = 0;
				bool flag3 = this.namespaceStack.Count > 1;
				if (flag3)
				{
					this.namespaceStack.Pop();
					parent = ((NamespaceInfo)this.namespaceStack.Peek()).NamespaceID;
					this.namespaceStack.Push(namespaceInfo);
				}
				namespaceInfo.NamespaceID = this.msw.DefineNamespace(namespaceInfo.Name, ((ICompileUnit)doc).Entry, using_clauses, parent);
			}
			return namespaceInfo.NamespaceID;
		}

		// Token: 0x040003FD RID: 1021
		private MonoSymbolWriter msw;

		// Token: 0x040003FE RID: 1022
		private int nextLocalIndex;

		// Token: 0x040003FF RID: 1023
		private int currentToken;

		// Token: 0x04000400 RID: 1024
		private string methodName;

		// Token: 0x04000401 RID: 1025
		private Stack namespaceStack = new Stack();

		// Token: 0x04000402 RID: 1026
		private bool methodOpened;

		// Token: 0x04000403 RID: 1027
		private Hashtable documents = new Hashtable();

		// Token: 0x04000404 RID: 1028
		private ModuleBuilder mb;

		// Token: 0x04000405 RID: 1029
		private SymbolWriterImpl.GetGuidFunc get_guid_func;

		// Token: 0x020000C1 RID: 193
		// (Invoke) Token: 0x060006AA RID: 1706
		private delegate Guid GetGuidFunc(ModuleBuilder mb);
	}
}
