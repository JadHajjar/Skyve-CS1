using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Skyve.Systems.CS1.Utilities;
internal static class LsmReportParser
{
	private static readonly Regex MissingEntryRegex = new(@"data-lomtag=""missing.*?""[^>]*href=""(.+?(\d+))"">(.+?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex UnusedEntryRegex = new(@"data-lomtag=""unused""[^>]*href=""(.+?(\d+))"">(.+?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex MultipleSpacesRegex = new(@"\s{2,}", RegexOptions.Compiled);
	public static IEnumerable<IPackageIdentity> ParseMissingAssets(string reportPath)
	{
		return ParseReport(reportPath, MissingEntryRegex, true);
	}

	public static IEnumerable<IPackageIdentity> ParseUnusedAssets(string reportPath)
	{
		return ParseReport(reportPath, UnusedEntryRegex, false);
	}

	private static IEnumerable<IPackageIdentity> ParseReport(string reportPath, Regex regex, bool isMissing)
	{
		var packageManager = ServiceCenter.Get<IPackageManager>();
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var lines = File.ReadAllLines(reportPath);

		foreach (var line in lines)
		{
			var match = regex.Match(line);

			if (!match.Success || !ulong.TryParse(match.Groups[2].Value, out var steamId))
			{
				continue;
			}

			var rawName = match.Groups[3].Value;
			var decoded = WebUtility.HtmlDecode(rawName).Trim();
			var normalizedTarget = NormalizeForComparison(decoded);

			var entry = ResolveEntry(packageManager, steamId, normalizedTarget, decoded, isMissing, out var seenKey);

			if (!seen.Add(seenKey))
			{
				continue;
			}

			yield return entry;
		}
	}

	private static IPackageIdentity ResolveEntry(IPackageManager packageManager, ulong steamId, string normalizedTarget, string displayName, bool isMissing, out string seenKey)
	{
		var asset = ResolveAsset(packageManager, steamId, normalizedTarget);

		if (asset is not null)
		{
			seenKey = asset.FilePath.FormatPath();
			return asset;
		}

		seenKey = $"lsm-unresolved:{steamId}:{normalizedTarget}";
		return new UnknownLsmReportIdentity(steamId, displayName, isMissing);
	}

	private static IAsset? ResolveAsset(IPackageManager packageManager, ulong steamId, string normalizedTarget)
	{
		if (packageManager.GetPackageById(new GenericPackageIdentity(steamId)) is not ILocalPackageWithContents package)
		{
			return null;
		}

		if (package.Assets is null || package.Assets.Length == 0)
		{
			return null;
		}

		foreach (var asset in package.Assets)
		{
			if (asset is not Asset assetWithFullName || string.IsNullOrWhiteSpace(assetWithFullName.FullName))
			{
				continue;
			}

			var normalized = NormalizeAssetIdentifier(assetWithFullName.FullName, steamId);

			if (string.Equals(normalized, normalizedTarget, StringComparison.OrdinalIgnoreCase))
			{
				return asset;
			}
		}

		return null;
	}

	private static string NormalizeAssetIdentifier(string value, ulong steamId)
	{
		var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

		if (steamId != 0)
		{
			var prefix = $"{steamId}.";

			if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				normalized = normalized.Substring(prefix.Length);
			}
		}

		return NormalizeForComparison(normalized);
	}

	private static string NormalizeForComparison(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		const string dataSuffix = "_Data";

		if (value.EndsWith(dataSuffix, StringComparison.OrdinalIgnoreCase))
		{
			value = value.Substring(0, value.Length - dataSuffix.Length);
		}

		value = value.Replace('_', ' ');
		value = MultipleSpacesRegex.Replace(value, " ");

		return value.Trim();
	}
}

internal sealed class UnknownLsmReportIdentity : IPackageIdentity
{
	public UnknownLsmReportIdentity(ulong steamId, string shortName, bool isMissing)
	{
		Id = steamId;
		ShortName = shortName;
		IsMissing = isMissing;
		Name = $"[{(isMissing ? "Missing" : "Unused")}] {shortName}";
		Url = $"https://steamcommunity.com/workshop/filedetails/?id={steamId}";
	}

	public ulong Id { get; }
	public string Name { get; }
	public string Url { get; }
	public string ShortName { get; }
	public bool IsMissing { get; }
}
