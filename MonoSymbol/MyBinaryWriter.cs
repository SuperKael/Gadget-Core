using System.IO;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000A6 RID: 166
	internal sealed class MyBinaryWriter : BinaryWriter
	{
		// Token: 0x060005D9 RID: 1497 RVA: 0x00025C4C File Offset: 0x00023E4C
		public MyBinaryWriter(Stream stream) : base(stream)
		{
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00025C58 File Offset: 0x00023E58
		public void WriteLeb128(int value)
		{
			base.Write7BitEncodedInt(value);
		}
	}
}
