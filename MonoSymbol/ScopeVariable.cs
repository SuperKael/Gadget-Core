namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B6 RID: 182
	internal struct ScopeVariable
	{
		// Token: 0x06000627 RID: 1575 RVA: 0x00027574 File Offset: 0x00025774
		public ScopeVariable(int scope, int index)
		{
			this.Scope = scope;
			this.Index = index;
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00027588 File Offset: 0x00025788
		internal ScopeVariable(MyBinaryReader reader)
		{
			this.Scope = reader.ReadLeb128();
			this.Index = reader.ReadLeb128();
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x000275A4 File Offset: 0x000257A4
		internal void Write(MyBinaryWriter bw)
		{
			bw.WriteLeb128(this.Scope);
			bw.WriteLeb128(this.Index);
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x000275C4 File Offset: 0x000257C4
		public override string ToString()
		{
			return string.Format("[ScopeVariable {0}:{1}]", this.Scope, this.Index);
		}

		// Token: 0x040003AD RID: 941
		public readonly int Scope;

		// Token: 0x040003AE RID: 942
		public readonly int Index;
	}
}
