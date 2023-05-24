using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class Underline : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Underline;
}
