using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Systems;
using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Skyve.Systems.CS1.Utilities;

internal static class LsmReportParser
{
	private static readonly Regex UnusedEntryRegex = CreateEntryRegex("unused");

	public static IEnumerable<IPackageIdentity> ParseUnusedAssets(string reportPath)
		=> ParseReport(reportPath, UnusedEntryRegex, "Unused");

	private static IEnumerable<IPackageIdentity> ParseReport(string reportPath, Regex regex, string reportKind)
	{
		if (!TryGetPackageManager(out var packageManager))
		{
			yield break;
		}

		if (!TryReadReport(reportPath, reportKind, out var reportContents))
		{
			yield break;
		}

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var entry in EnumerateEntries(regex, reportContents))
		{
			var identity = ResolveEntrySafely(packageManager, entry, out var dedupeKey);

			if (seen.Add(dedupeKey))
			{
				yield return identity;
			}
		}
	}

	private static bool TryGetPackageManager(out IPackageManager packageManager)
	{
		try
		{
			packageManager = ServiceCenter.Get<IPackageManager>();
			return true;
		}
		catch (Exception ex)
		{
			LogCritical(ex, "LSM report parser could not resolve IPackageManager.");
			packageManager = null!;
			return false;
		}
	}

	private static bool TryReadReport(string reportPath, string kind, out string reportContents)
	{
		try
		{
			reportContents = File.ReadAllText(reportPath);
			return true;
		}
		catch (Exception ex)
		{
			LogCritical(ex, $"LSM report parse failed. Path='{reportPath}', Kind='{kind}'.");
			reportContents = string.Empty;
			return false;
		}
	}

	private readonly struct LsmEntry
	{
		public LsmEntry(ulong steamId, string displayName, string normalizedTarget)
		{
			SteamId = steamId;
			DisplayName = displayName;
			NormalizedTarget = normalizedTarget;
		}

		public ulong SteamId { get; }
		public string DisplayName { get; }
		public string NormalizedTarget { get; }
	}

	private static IEnumerable<LsmEntry> EnumerateEntries(Regex regex, string reportContents)
	{
		foreach (Match match in regex.Matches(reportContents))
		{
			if (!match.Success)
			{
				continue;
			}

			var idText = match.Groups["id"].Value;
			if (!ulong.TryParse(idText, out var steamId))
			{
				continue;
			}

			var rawName = match.Groups["name"].Value;
			var displayName = WebUtility.HtmlDecode(rawName).Trim();
			var normalizedTarget = LsmNameNormalizer.NormalizeForComparison(displayName);

			yield return new LsmEntry(steamId, displayName, normalizedTarget);
		}
	}

	private static IPackageIdentity ResolveEntrySafely(
		IPackageManager packageManager,
		LsmEntry entry,
		out string dedupeKey)
	{
		try
		{
			return ResolveEntry(packageManager, entry, out dedupeKey);
		}
		catch (Exception ex)
		{
			LogCritical(ex, $"LSM entry resolution failed. SteamId={entry.SteamId}, Name='{entry.DisplayName}', Normalized='{entry.NormalizedTarget}'.");
			dedupeKey = BuildUnresolvedKey(entry.SteamId, entry.NormalizedTarget);
			return new UnknownLsmReportIdentity(entry.SteamId, entry.DisplayName);
		}
	}

	private static IPackageIdentity ResolveEntry(
		IPackageManager packageManager,
		LsmEntry entry,
		out string dedupeKey)
	{
		var asset = ResolveAsset(packageManager, entry.SteamId, entry.NormalizedTarget);

		if (asset is not null)
		{
			dedupeKey = asset.FilePath.FormatPath();
			return asset;
		}

		dedupeKey = BuildUnresolvedKey(entry.SteamId, entry.NormalizedTarget);
		return new UnknownLsmReportIdentity(entry.SteamId, entry.DisplayName);
	}
	private static string BuildUnresolvedKey(ulong steamId, string normalizedTarget)
		=> $"lsm-unresolved:{steamId}:{normalizedTarget}";

	private static IAsset? ResolveAsset(IPackageManager packageManager, ulong steamId, string normalizedTarget)
	{
		if (packageManager.GetPackageById(new GenericPackageIdentity(steamId)) is not ILocalPackageWithContents package)
		{
			return null;
		}

		var assets = package.Assets;
		if (assets is null || assets.Length == 0)
		{
			return null;
		}

		IAsset? fullNameFallback = null;

		foreach (var asset in assets)
		{
			if (asset is not Asset a)
			{
				continue;
			}

			if (TryMatch(LsmNameNormalizer.NormalizeForComparison(a.MetadataName), normalizedTarget))
			{
				return a;
			}

			if (TryMatch(LsmNameNormalizer.NormalizeForComparison(a.Name), normalizedTarget))
			{
				return a;
			}

			if (TryMatch(LsmNameNormalizer.NormalizeAssetIdentifier(a.FullName, steamId), normalizedTarget))
			{
				fullNameFallback ??= a;
			}
		}

		return fullNameFallback;
	}

	private static bool TryMatch(string normalizedValue, string normalizedTarget)
		=> normalizedValue.Length > 0 &&
		   string.Equals(normalizedValue, normalizedTarget, StringComparison.OrdinalIgnoreCase);

	private static Regex CreateEntryRegex(params string[] tags)
	{
		var pattern =
			$@"<a(?=[^>]*data-lomtag=""(?:{string.Join("|", tags)})"")[^>]*href=""[^""]*id=(?<id>\d+)""[^>]*>(?<name>.*?)</a>";

		return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}
	
	private static ILogger? TryGetLogger()
	{
		try { return ServiceCenter.Get<ILogger>(); }
		catch { return null; }
	}

	private static void LogCritical(Exception exception, string message)
	{
		var logger = TryGetLogger();
		if (logger is null) return;

		try { logger.Exception(exception, message); }
		catch {  }
	}
}

internal sealed class UnknownLsmReportIdentity : IPackageIdentity
{
	public UnknownLsmReportIdentity(ulong steamId, string name)
	{
		Id = steamId;
		Name = name;
		Url = $"https://steamcommunity.com/workshop/filedetails/?id={steamId}";
	}

	public ulong Id { get; }
	public string Name { get; }
	public string Url { get; }
}
