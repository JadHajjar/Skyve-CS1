using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Steam.Markdown
{
	internal class List : Component
	{
		private readonly List<string> entries = new List<string>();

		public override void Create(string value, string attributes)
		{
			var split = value.Split("[*]".ToCharArray());

			foreach (var entry in split)
			{
				if (entry.Trim().Length == 0)
				{
					continue;
				}

				this.entries.Add(entry.Trim());
			}
		}

		public List<string> GetItems()
		{
			return this.entries;
		}
	}
}
