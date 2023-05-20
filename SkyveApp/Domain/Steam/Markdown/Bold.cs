using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class Bold : PlainText
{
	protected override FontStyle FontStyle => FontStyle.Bold;
}
