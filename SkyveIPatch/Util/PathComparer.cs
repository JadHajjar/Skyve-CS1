using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyveShared;
public class PathEqualityComparer : IEqualityComparer<string>
{
	public bool Equals(string path1, string path2)
	{
		if (path1 == path2)
		{
			return true;
		}

		var normalizedPath1 = path1.Replace('\\', '/');
		var normalizedPath2 = path2.Replace('\\', '/');

		return string.Equals(normalizedPath1, normalizedPath2, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(string obj)
	{
		return obj
			.Replace('\\', '/')
			.ToLower()
			.GetHashCode();
	}
}