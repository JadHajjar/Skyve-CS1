using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LoadOrderToolTwo.Domain.Steam.Markdown
{
	public class BBCode
	{
		internal static Component Parse(string text) => GenerateComponent(new Component(), text);

		private static Component GenerateComponent(Component component, string text)
		{
			var tags = Regex.Matches(text, @"\[(\w+)(=.+?)?\](.+?)\[\/\1]", RegexOptions.Singleline);
			component.Children = new List<Component>();
			var index = 0;

			if (tags.Count == 0)
			{
				component.Text = text;
				return component;
			}

			for (var tag = 0; tag < tags.Count; tag++)
			{
				var subText = text.Substring(index, tags[tag].Index - index);

				if (subText.Length != 0)
				{
					component.Children.Add(new PlainText { Text = subText });
				}

				component.Children.Add(GenerateComponent(GetComponent(tags[tag]), tags[tag].Groups[3].Value));

				index = tags[tag].Index + tags[tag].Length;
			}

			if (index < text.Length)
			{
				component.Children.Add(new PlainText { Text = text.Substring(index) });
			}

			return component;
		}

		private static Component GetComponent(Match match)
		{
			Component component = match.Groups[1].Value switch
			{
				"u" => new Underline(),
				"i" => new Italic(),
				"b" => new Bold(),
				"strike" => new Strike(),
				"h1" => new Title(),
				"h2" => new SubTitle(),
				"h3" => new SubSubTitle(),
				"url" => new URL(),
				"spoiler" => new Spoiler(),
				"noparse" => new NoParse(),
				"code" => new Code(),
				"quote" => new Quote(),
				"olist" => new List(),
				"list" => new List(),
				"img" => new Image(),
				_ => new PlainText(),
			};

			if (match.Groups[2].Value.Length > 0)
				component.Argument = match.Groups[2].Value.Substring(1);

			return component;
		}
	}
}
