namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B2 RID: 178
	internal struct LocalVariableEntry
	{
		// Token: 0x0600061B RID: 1563 RVA: 0x000273AC File Offset: 0x000255AC
		public LocalVariableEntry(int index, string name, int block)
		{
			this.Index = index;
			this.Name = name;
			this.BlockIndex = block;
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x000273C4 File Offset: 0x000255C4
		internal LocalVariableEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.Index = reader.ReadLeb128();
			this.Name = reader.ReadString();
			this.BlockIndex = reader.ReadLeb128();
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x000273EC File Offset: 0x000255EC
		internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
		{
			bw.WriteLeb128(this.Index);
			bw.Write(this.Name);
			bw.WriteLeb128(this.BlockIndex);
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00027418 File Offset: 0x00025618
		public override string ToString()
		{
			return string.Format("[LocalVariable {0}:{1}:{2}]", this.Name, this.Index, this.BlockIndex - 1);
		}

		// Token: 0x040003A1 RID: 929
		public readonly int Index;

		// Token: 0x040003A2 RID: 930
		public readonly string Name;

		// Token: 0x040003A3 RID: 931
		public readonly int BlockIndex;
	}
}
