using System.Collections.Generic;

namespace GadgetCore.MonoSymbol
{
	// Token: 0x020000AE RID: 174
	internal class LineNumberEntry
	{
		// Token: 0x0600060D RID: 1549 RVA: 0x00027100 File Offset: 0x00025300
		public LineNumberEntry(int file, int row, int column, int offset) : this(file, row, column, offset, false)
		{
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00027110 File Offset: 0x00025310
		public LineNumberEntry(int file, int row, int offset) : this(file, row, -1, offset, false)
		{
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00027120 File Offset: 0x00025320
		public LineNumberEntry(int file, int row, int column, int offset, bool is_hidden) : this(file, row, column, -1, -1, offset, is_hidden)
		{
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x00027134 File Offset: 0x00025334
		public LineNumberEntry(int file, int row, int column, int end_row, int end_column, int offset, bool is_hidden)
		{
			this.File = file;
			this.Row = row;
			this.Column = column;
			this.EndRow = end_row;
			this.EndColumn = end_column;
			this.Offset = offset;
			this.IsHidden = is_hidden;
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x00027174 File Offset: 0x00025374
		public override string ToString()
		{
			return string.Format("[Line {0}:{1,2}-{3,4}:{5}]", new object[]
			{
				this.File,
				this.Row,
				this.Column,
				this.EndRow,
				this.EndColumn,
				this.Offset
			});
		}

		// Token: 0x0400038E RID: 910
		public readonly int Row;

		// Token: 0x0400038F RID: 911
		public int Column;

		// Token: 0x04000390 RID: 912
		public int EndRow;

		// Token: 0x04000391 RID: 913
		public int EndColumn;

		// Token: 0x04000392 RID: 914
		public readonly int File;

		// Token: 0x04000393 RID: 915
		public readonly int Offset;

		// Token: 0x04000394 RID: 916
		public readonly bool IsHidden;

		// Token: 0x04000395 RID: 917
		public static readonly LineNumberEntry Null = new LineNumberEntry(0, 0, 0, 0);

		// Token: 0x020000AF RID: 175
		public sealed class LocationComparer : IComparer<LineNumberEntry>
		{
			// Token: 0x06000613 RID: 1555 RVA: 0x000271FC File Offset: 0x000253FC
			public int Compare(LineNumberEntry l1, LineNumberEntry l2)
			{
				return (l1.Row == l2.Row) ? l1.Column.CompareTo(l2.Column) : l1.Row.CompareTo(l2.Row);
			}

			// Token: 0x04000396 RID: 918
			public static readonly LineNumberEntry.LocationComparer Default = new LineNumberEntry.LocationComparer();
		}
	}
}
