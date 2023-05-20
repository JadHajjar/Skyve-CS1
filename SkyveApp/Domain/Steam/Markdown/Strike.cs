using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class Strike : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Strikeout;
}
