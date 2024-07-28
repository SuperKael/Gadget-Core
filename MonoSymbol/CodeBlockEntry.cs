namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000B0 RID: 176
	internal class CodeBlockEntry
	{
		// Token: 0x06000616 RID: 1558 RVA: 0x0002725C File Offset: 0x0002545C
		public CodeBlockEntry(int index, int parent, CodeBlockEntry.Type type, int start_offset)
		{
			this.Index = index;
			this.Parent = parent;
			this.BlockType = type;
			this.StartOffset = start_offset;
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00027284 File Offset: 0x00025484
		internal CodeBlockEntry(int index, MyBinaryReader reader)
		{
			this.Index = index;
			int num = reader.ReadLeb128();
			this.BlockType = (CodeBlockEntry.Type)(num & 63);
			this.Parent = reader.ReadLeb128();
			this.StartOffset = reader.ReadLeb128();
			this.EndOffset = reader.ReadLeb128();
			bool flag = (num & 64) != 0;
			if (flag)
			{
				int num2 = (int)reader.ReadInt16();
				reader.BaseStream.Position += (long)num2;
			}
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x00027300 File Offset: 0x00025500
		public void Close(int end_offset)
		{
			this.EndOffset = end_offset;
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x0002730C File Offset: 0x0002550C
		internal void Write(MyBinaryWriter bw)
		{
			bw.WriteLeb128((int)this.BlockType);
			bw.WriteLeb128(this.Parent);
			bw.WriteLeb128(this.StartOffset);
			bw.WriteLeb128(this.EndOffset);
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x00027344 File Offset: 0x00025544
		public override string ToString()
		{
			return string.Format("[CodeBlock {0}:{1}:{2}:{3}:{4}]", new object[]
			{
				this.Index,
				this.Parent,
				this.BlockType,
				this.StartOffset,
				this.EndOffset
			});
		}

		// Token: 0x04000397 RID: 919
		public int Index;

		// Token: 0x04000398 RID: 920
		public int Parent;

		// Token: 0x04000399 RID: 921
		public CodeBlockEntry.Type BlockType;

		// Token: 0x0400039A RID: 922
		public int StartOffset;

		// Token: 0x0400039B RID: 923
		public int EndOffset;

		// Token: 0x020000B1 RID: 177
		public enum Type
		{
			// Token: 0x0400039D RID: 925
			Lexical = 1,
			// Token: 0x0400039E RID: 926
			CompilerGenerated,
			// Token: 0x0400039F RID: 927
			IteratorBody,
			// Token: 0x040003A0 RID: 928
			IteratorDispatcher
		}
	}
}
