namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000C3 RID: 195
	internal class SourceMethodImpl : IMethodDef
	{
		// Token: 0x060006B2 RID: 1714 RVA: 0x0002A0D8 File Offset: 0x000282D8
		public SourceMethodImpl(string name, int token, int namespaceID)
		{
			this.name = name;
			this.token = token;
			this.namespaceID = namespaceID;
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060006B3 RID: 1715 RVA: 0x0002A0F8 File Offset: 0x000282F8
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060006B4 RID: 1716 RVA: 0x0002A110 File Offset: 0x00028310
		public int NamespaceID
		{
			get
			{
				return this.namespaceID;
			}
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060006B5 RID: 1717 RVA: 0x0002A128 File Offset: 0x00028328
		public int Token
		{
			get
			{
				return this.token;
			}
		}

		// Token: 0x04000407 RID: 1031
		private string name;

		// Token: 0x04000408 RID: 1032
		private int token;

		// Token: 0x04000409 RID: 1033
		private int namespaceID;
	}
}
