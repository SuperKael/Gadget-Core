namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B3 RID: 179
	internal struct CapturedVariable
	{
		// Token: 0x0600061F RID: 1567 RVA: 0x00027454 File Offset: 0x00025654
		public CapturedVariable(string name, string captured_name, CapturedVariable.CapturedKind kind)
		{
			this.Name = name;
			this.CapturedName = captured_name;
			this.Kind = kind;
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x0002746C File Offset: 0x0002566C
		internal CapturedVariable(MyBinaryReader reader)
		{
			this.Name = reader.ReadString();
			this.CapturedName = reader.ReadString();
			this.Kind = (CapturedVariable.CapturedKind)reader.ReadByte();
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00027494 File Offset: 0x00025694
		internal void Write(MyBinaryWriter bw)
		{
			bw.Write(this.Name);
			bw.Write(this.CapturedName);
			bw.Write((byte)this.Kind);
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x000274C0 File Offset: 0x000256C0
		public override string ToString()
		{
			return string.Format("[CapturedVariable {0}:{1}:{2}]", this.Name, this.CapturedName, this.Kind);
		}

		// Token: 0x040003A4 RID: 932
		public readonly string Name;

		// Token: 0x040003A5 RID: 933
		public readonly string CapturedName;

		// Token: 0x040003A6 RID: 934
		public readonly CapturedVariable.CapturedKind Kind;

		// Token: 0x020000B4 RID: 180
		public enum CapturedKind : byte
		{
			// Token: 0x040003A8 RID: 936
			Local,
			// Token: 0x040003A9 RID: 937
			Parameter,
			// Token: 0x040003AA RID: 938
			This
		}
	}
}
