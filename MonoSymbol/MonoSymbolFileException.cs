using System;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000A5 RID: 165
	internal class MonoSymbolFileException : Exception
	{
		// Token: 0x060005D6 RID: 1494 RVA: 0x00025C20 File Offset: 0x00023E20
		public MonoSymbolFileException()
		{
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x00025C2C File Offset: 0x00023E2C
		public MonoSymbolFileException(string message, params object[] args) : base(string.Format(message, args))
		{
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00025C40 File Offset: 0x00023E40
		public MonoSymbolFileException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
