using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Steam.Markdown
{
	internal class Component
	{
		protected string value = null;

		public List<Component> Children { get; internal set; }
		public string Argument { get; internal set; }
		public string Text { get; internal set; }

		public virtual void Create(string value, string attributes)
		{
			this.value = value;
		}

		public string GetContent()
		{
			return value;
		}
	}
}
