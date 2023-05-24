using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Utilities;
using SkyveApp.Utilities.Managers;

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SkyveApp.Utilities;
internal static class LsmUtil
{
	public static bool IsValidLsmReportFile(string filePath)
	{
		if (!ExtensionClass.FileExists(filePath) || new FileInfo(filePath).Length > 50 * 1024 * 1024)
		{
			return false;
		}

		using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		using var streamReader = new StreamReader(fileStream);

		while (!streamReader.EndOfStream)
		{
			var line = streamReader.ReadLine();

			if (Regex.IsMatch(line, "data-lomtag=\"(missing.*?)|(unused)\".+?href=\"(.+?(\\d+))\">(.+?)</a>"))
			{
				return true;
			}
		}

		return false;
	}

	internal static string GetReportFolder()
	{
		var path = LsmSettingsFile.Deserialize()?.reportDir;

		if (path is null)
		{
			return string.Empty;
		}

		var regex = Regex.Match(path, @"Colossal Order[\\/]Cities_Skylines[\\/]", RegexOptions.IgnoreCase);

		if (regex.Success) // attempt to match the file to any OS or user
		{
			path = LocationManager.Combine(LocationManager.AppDataPath, path.Substring(regex.Index + regex.Length));
		}

		return path;
	}

	internal static IEnumerable<Profile.Asset> LoadMissingAssets(string obj)
	{
		var lines = File.ReadAllLines(obj);

		for (var i = 0; i < lines.Length; i++)
		{
			var match = Regex.Match(lines[i], "data-lomtag=\"missing.*?\".+?href=\"(.+?(\\d+))\">(.+?)</a>");

			if (match.Success)
			{
				var steamId = match.Groups[2].Value;
				var assetName = System.Net.WebUtility.HtmlDecode(match.Groups[3].Value);

				yield return new Profile.Asset
				{
					SteamId = ulong.Parse(steamId),
					Name = assetName
				};
			}
		}
	}

	internal static IEnumerable<Profile.Asset> LoadUnusedAssets(string obj)
	{
		var lines = File.ReadAllLines(obj);

		for (var i = 0; i < lines.Length; i++)
		{
			var match = Regex.Match(lines[i], "data-lomtag=\"unused\".+?href=\"(.+?(\\d+))\">(.+?)</a>");

			if (match.Success)
			{
				var steamId = match.Groups[2].Value;
				var assetName = System.Net.WebUtility.HtmlDecode(match.Groups[3].Value);

				yield return new Profile.Asset
				{
					SteamId = ulong.Parse(steamId),
					Name = assetName
				};
			}
		}
	}
}
