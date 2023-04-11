using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Underline : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Underline;
}
