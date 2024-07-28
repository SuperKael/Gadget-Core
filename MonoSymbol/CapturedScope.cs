namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B5 RID: 181
	internal struct CapturedScope
	{
		// Token: 0x06000623 RID: 1571 RVA: 0x000274F4 File Offset: 0x000256F4
		public CapturedScope(int scope, string captured_name)
		{
			this.Scope = scope;
			this.CapturedName = captured_name;
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00027508 File Offset: 0x00025708
		internal CapturedScope(MyBinaryReader reader)
		{
			this.Scope = reader.ReadLeb128();
			this.CapturedName = reader.ReadString();
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00027524 File Offset: 0x00025724
		internal void Write(MyBinaryWriter bw)
		{
			bw.WriteLeb128(this.Scope);
			bw.Write(this.CapturedName);
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x00027544 File Offset: 0x00025744
		public override string ToString()
		{
			return string.Format("[CapturedScope {0}:{1}]", this.Scope, this.CapturedName);
		}

		// Token: 0x040003AB RID: 939
		public readonly int Scope;

		// Token: 0x040003AC RID: 940
		public readonly string CapturedName;
	}
}
