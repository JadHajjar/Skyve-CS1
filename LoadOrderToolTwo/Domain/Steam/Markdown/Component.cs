using Extensions;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Component
{
	protected string? value = null;

	public List<Component>? Children { get; internal set; }
	public string? Argument { get; internal set; }
	public string? Text { get; internal set; }

	public virtual void Create(string value, string attributes)
	{
		this.value = value;
	}

	public virtual void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		if (Children is not null)
		{
			foreach (var item in Children)
			{
				item.Draw(g, draw, hoverState, mouse, width, ref height);
			}
		}
	}

	public string? GetContent()
	{
		return value;
	}
}
