using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Bold : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Bold;
}
