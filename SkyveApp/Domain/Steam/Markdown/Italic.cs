using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class Italic : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Italic;
}
