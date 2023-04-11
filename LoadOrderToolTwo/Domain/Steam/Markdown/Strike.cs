using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Strike : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Strikeout;
}
