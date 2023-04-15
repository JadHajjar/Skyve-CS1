using SlickControls;

using System.Collections.Generic;
using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class List : Component
{
	private readonly bool _ordered;

	public List(bool ordered)
	{
		_ordered = ordered;
	}

	internal static IEnumerable<string> SplitChildren(string text)
	{
		var currentItemIndex = -1;
		var currentNesting = 0;

		for (var i = 0; i < text.Length; i++)
		{
			if (i + 7 < text.Length)
			{
				if (text.Substring(i, 6).ToLower() == "[list]")
				{
					currentNesting++;
				}
				else if (text.Substring(i, 7).ToLower() == "[/list]")
				{
					currentNesting--;
				}
			}

			if (i + 2 < text.Length)
			{
				if (currentNesting == 0 && text[i] == '[' && text[i + 1] == '*' && text[i + 2] == ']')
				{
					if (currentItemIndex != -1)
					{
						yield return text.Substring(currentItemIndex, i - currentItemIndex);
					}

					currentItemIndex = i + 3;
				}
			}
		}
	}

	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
