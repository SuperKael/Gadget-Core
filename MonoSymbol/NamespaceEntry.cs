namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000BD RID: 189
	internal struct NamespaceEntry
	{
		// Token: 0x06000665 RID: 1637 RVA: 0x000291E4 File Offset: 0x000273E4
		public NamespaceEntry(string name, int index, string[] using_clauses, int parent)
		{
			this.Name = name;
			this.Index = index;
			this.Parent = parent;
			this.UsingClauses = ((using_clauses != null) ? using_clauses : new string[0]);
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x00029210 File Offset: 0x00027410
		internal NamespaceEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.Name = reader.ReadString();
			this.Index = reader.ReadLeb128();
			this.Parent = reader.ReadLeb128();
			int num = reader.ReadLeb128();
			this.UsingClauses = new string[num];
			for (int i = 0; i < num; i++)
			{
				this.UsingClauses[i] = reader.ReadString();
			}
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x00029274 File Offset: 0x00027474
		internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
		{
			bw.Write(this.Name);
			bw.WriteLeb128(this.Index);
			bw.WriteLeb128(this.Parent);
			bw.WriteLeb128(this.UsingClauses.Length);
			foreach (string value in this.UsingClauses)
			{
				bw.Write(value);
			}
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x000292DC File Offset: 0x000274DC
		public override string ToString()
		{
			return string.Format("[Namespace {0}:{1}:{2}]", this.Name, this.Index, this.Parent);
		}

		// Token: 0x040003EA RID: 1002
		public readonly string Name;

		// Token: 0x040003EB RID: 1003
		public readonly int Index;

		// Token: 0x040003EC RID: 1004
		public readonly int Parent;

		// Token: 0x040003ED RID: 1005
		public readonly string[] UsingClauses;
	}
}
