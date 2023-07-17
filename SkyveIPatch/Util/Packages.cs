using SkyveShared;

using System.Collections.Generic;

namespace SkyveIPatch;
public static class Packages
{
	private static HashSet<string> excludedPathsLowerCase_;

	private static HashSet<string> Create()
	{
		var config = AssetConfig.Deserialize();

		return new HashSet<string>(config.ExcludedAssets, new PathEqualityComparer());
	}

	private static HashSet<string> ExcludedPathsLowerCase => excludedPathsLowerCase_ ??= Create();

	public static bool IsFileExcluded(string path)
	{
		return string.IsNullOrEmpty(path) || path.Contains(@"\_") || path.Contains(@"/_") || IsPathExcluded(path);
	}

	public static bool IsPathExcluded(string path)
	{
		return ExcludedPathsLowerCase.Contains(path);
	}
}
