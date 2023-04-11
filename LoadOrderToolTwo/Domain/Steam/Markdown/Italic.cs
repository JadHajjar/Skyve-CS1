using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Italic : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Italic;
}
