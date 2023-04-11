using Extensions;

using LoadOrderToolTwo.Domain.Steam.Markdown;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MdImage = LoadOrderToolTwo.Domain.Steam.Markdown.Image;

namespace LoadOrderToolTwo.UserInterface.Generic;
internal class SteamDescriptionViewer : SlickControl
{
	public Component Markdown { get; }
	public List<string> Images { get; }

	public SteamDescriptionViewer(string description)
	{
		Markdown = BBCode.Parse(description);
		TabStop = false;
		Height = GetHeight();

		Images = GetAllImages(Markdown).ToList();
	}

	private IEnumerable<string> GetAllImages(Component component)
	{
		if (component is MdImage image)
		{
			yield return image.GetURL();
		}

		if (component.Children is not null)
		{
			foreach (var comp in component.Children)
			{
				foreach (var item in GetAllImages(comp))
				{
					yield return item;
				}
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		DrawItems(e.Graphics, true);
	}

	private int GetHeight()
	{
		return DrawItems(Graphics.FromHwnd(IntPtr.Zero), false);
	}

	private int DrawItems(Graphics g, bool draw)
	{
		var h = (int)(6 * UI.FontScale);
		var mouse = PointToClient(Cursor.Position);

		Markdown.Draw(g, draw, HoverState, mouse, Width, ref h);

		return h + (int)(6 * UI.FontScale);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);

		var ch = GetHeight();

		if (Height != ch)
		{
			Height = ch;
		}
	}
}