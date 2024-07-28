using System.IO;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000A7 RID: 167
	internal class MyBinaryReader : BinaryReader
	{
		// Token: 0x060005DB RID: 1499 RVA: 0x00025C64 File Offset: 0x00023E64
		public MyBinaryReader(Stream stream) : base(stream)
		{
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x00025C70 File Offset: 0x00023E70
		public int ReadLeb128()
		{
			return base.Read7BitEncodedInt();
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x00025C88 File Offset: 0x00023E88
		public string ReadString(int offset)
		{
			long position = this.BaseStream.Position;
			this.BaseStream.Position = (long)offset;
			string result = this.ReadString();
			this.BaseStream.Position = position;
			return result;
		}
	}
}
