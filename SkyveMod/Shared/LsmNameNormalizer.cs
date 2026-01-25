using System;
using System.Text.RegularExpressions;

namespace SkyveShared;
public static class LsmNameNormalizer
{
	private static readonly Regex MultipleSpacesRegex = new(@"\s{2,}", RegexOptions.Compiled);
	
	public static string NormalizeForComparison(string? value)
	{
		if (IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		string normalized = value.Trim();
		const string dataSuffix = "_Data";

		if (normalized.EndsWith(dataSuffix, StringComparison.OrdinalIgnoreCase))
		{
			normalized = normalized.Substring(0, normalized.Length - dataSuffix.Length);
		}

		normalized = normalized.Replace('_', ' ');
		normalized = MultipleSpacesRegex.Replace(normalized, " ");
		return normalized.Trim();
	}
	public static string NormalizeAssetIdentifier(string? value, ulong steamId)
	{
		if (IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		string normalized = value.Trim();

		if (steamId != 0)
		{
			string prefix = $"{steamId}.";

			if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				normalized = normalized.Substring(prefix.Length);
			}
		}

		return NormalizeForComparison(normalized);
	}
	private static bool IsNullOrWhiteSpace(string? value)
	{
		return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
	}
}
