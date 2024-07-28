using System.Collections.Generic;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B7 RID: 183
	internal class AnonymousScopeEntry
	{
		// Token: 0x0600062B RID: 1579 RVA: 0x000275F8 File Offset: 0x000257F8
		public AnonymousScopeEntry(int id)
		{
			this.ID = id;
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x00027620 File Offset: 0x00025820
		internal AnonymousScopeEntry(MyBinaryReader reader)
		{
			this.ID = reader.ReadLeb128();
			int num = reader.ReadLeb128();
			for (int i = 0; i < num; i++)
			{
				this.captured_vars.Add(new CapturedVariable(reader));
			}
			int num2 = reader.ReadLeb128();
			for (int j = 0; j < num2; j++)
			{
				this.captured_scopes.Add(new CapturedScope(reader));
			}
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x000276B0 File Offset: 0x000258B0
		internal void AddCapturedVariable(string name, string captured_name, CapturedVariable.CapturedKind kind)
		{
			this.captured_vars.Add(new CapturedVariable(name, captured_name, kind));
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x000276C8 File Offset: 0x000258C8
		public CapturedVariable[] CapturedVariables
		{
			get
			{
				CapturedVariable[] array = new CapturedVariable[this.captured_vars.Count];
				this.captured_vars.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x000276FC File Offset: 0x000258FC
		internal void AddCapturedScope(int scope, string captured_name)
		{
			this.captured_scopes.Add(new CapturedScope(scope, captured_name));
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x00027714 File Offset: 0x00025914
		public CapturedScope[] CapturedScopes
		{
			get
			{
				CapturedScope[] array = new CapturedScope[this.captured_scopes.Count];
				this.captured_scopes.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x00027748 File Offset: 0x00025948
		internal void Write(MyBinaryWriter bw)
		{
			bw.WriteLeb128(this.ID);
			bw.WriteLeb128(this.captured_vars.Count);
			foreach (CapturedVariable capturedVariable in this.captured_vars)
			{
				capturedVariable.Write(bw);
			}
			bw.WriteLeb128(this.captured_scopes.Count);
			foreach (CapturedScope capturedScope in this.captured_scopes)
			{
				capturedScope.Write(bw);
			}
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x00027818 File Offset: 0x00025A18
		public override string ToString()
		{
			return string.Format("[AnonymousScope {0}]", this.ID);
		}

		// Token: 0x040003AF RID: 943
		public readonly int ID;

		// Token: 0x040003B0 RID: 944
		private List<CapturedVariable> captured_vars = new List<CapturedVariable>();

		// Token: 0x040003B1 RID: 945
		private List<CapturedScope> captured_scopes = new List<CapturedScope>();
	}
}
