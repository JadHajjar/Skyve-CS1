using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Utilities;
internal static class LsmUtil
{
	internal static IEnumerable<Profile.Asset> LoadMissingAssets(string obj)
	{
		var lines = File.ReadAllLines(obj);

		for (int i = 0; i < lines.Length; i++)
		{
			var match = Regex.Match(lines[i], "data-lomtag=\"missingreq\".+?href=\"(.+?(\\d+))\">(.+?)</a>");

			if (match.Success)
			{
				var steamUrl = match.Groups[1].Value;
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
