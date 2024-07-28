using System;
using System.Diagnostics.SymbolStore;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000C2 RID: 194
	internal class SymbolDocumentWriterImpl : ISymbolDocumentWriter, ISourceFile, ICompileUnit
	{
		// Token: 0x060006AD RID: 1709 RVA: 0x0002A084 File Offset: 0x00028284
		public SymbolDocumentWriterImpl(CompileUnitEntry comp_unit)
		{
			this.comp_unit = comp_unit;
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x0002A098 File Offset: 0x00028298
		public void SetCheckSum(Guid algorithmId, byte[] checkSum)
		{
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x0002A09C File Offset: 0x0002829C
		public void SetSource(byte[] source)
		{
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060006B0 RID: 1712 RVA: 0x0002A0A0 File Offset: 0x000282A0
		SourceFileEntry ISourceFile.Entry
		{
			get
			{
				return this.comp_unit.SourceFile;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060006B1 RID: 1713 RVA: 0x0002A0C0 File Offset: 0x000282C0
		public CompileUnitEntry Entry
		{
			get
			{
				return this.comp_unit;
			}
		}

		// Token: 0x04000406 RID: 1030
		private CompileUnitEntry comp_unit;
	}
}
