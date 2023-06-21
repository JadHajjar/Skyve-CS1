using Extensions;

using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Services.Interfaces;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyveApp.Utilities;

internal static class ModExtensions
{
	internal static string CleanName(this IPackage package)
	{
		if (package?.Name is null)
		{
			return string.Empty;
		}

		var text = package.Name.RegexRemove(@"(?<!Catalogue\s+)v?\d+\.\d+(\.\d+)*(-[\d\w]+)*");

		return text.RegexRemove(@"[\[\(](.+?)[\]\)]").RemoveDoubleSpaces().Trim('-', ']', '[', '(', ')', ' ');
	}

	internal static string CleanName(this IPackage package, out List<(Color Color, string Text)> tags)
	{
		tags = new();

		var text = package?.Name.RegexRemove(@"v?\d+\.\d+(\.\d+)*(-[\d\w]+)*") ?? Locale.UnknownPackage;
		var tagMatches = Regex.Matches(text, @"[\[\(](.+?)[\]\)]");

		text = text.RegexRemove(@"[\[\(](.+?)[\]\)]").RemoveDoubleSpaces().Trim('-', ']', '[', '(', ')', ' ');

		if (package?.Workshop == false)
		{
			tags.Add((FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.AccentColor).MergeColor(FormDesign.Design.BackColor, 65), Locale.Local.One.ToUpper()));
		}

		foreach (Match match in tagMatches)
		{
			var tagText = match.Groups[1].Value.Trim();

			if (!tags.Any(x => x.Text == tagText))
			{
				if (tagText.ToLower() is "stable" or "deprecated" or "obsolete" or "abandoned" or "broken")
				{ continue; }

				var color = tagText.ToLower() switch
				{
					"alpha" or "experimental" => Color.FromArgb(200, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor)),
					"beta" or "test" or "testing" => Color.FromArgb(180, FormDesign.Design.YellowColor),
					_ => (Color?)null
				};

				if (package?.Workshop == false && color is not null)
				{
					continue;
				}

				tags.Add((color ?? FormDesign.Design.ButtonColor, color is null ? tagText : LocaleHelper.GetGlobalText(tagText).One.ToUpper()));
			}
		}

		if (package is null)
		{
			return text;
		}

		if (package.Banned)
		{
			tags.Add((FormDesign.Design.RedColor, LocaleCR.Banned.One.ToUpper()));
		}
		else if (package.Incompatible)
		{
			tags.Add((FormDesign.Design.RedColor, LocaleCR.Incompatible.One.ToUpper()));
		}
		else
		{
			var compatibility = package.GetCompatibilityInfo();

			if (compatibility.Data?.Package.Stability is PackageStability.Broken)
			{
				tags.Add((Color.FromArgb(225, FormDesign.Design.RedColor), LocaleCR.Broken.One.ToUpper()));
			}
		}

		return text;
	}

	internal static string GetVersionText(this string name)
	{
		var match = Regex.Match(name, @"v?(\d+\.\d+(\.\d+)*(-[\d\w]+)*)", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			return "v" + match.Groups[1].Value;
		}

		return string.Empty;
	}

	internal static IEnumerable<IPackage> GetPackagesThatReference(this IPackage package)
	{
		var settings = Program.Services.GetService<ISettings>();
		var contentManager = Program.Services.GetService<IContentManager>();
		var compatibilityUtil = Program.Services.GetService<ICompatibilityUtil>();
		
		var packages = settings.SessionSettings.UserSettings.ShowAllReferencedPackages ? contentManager.Packages.ToList() : contentManager.Packages.AllWhere(x => x.IsIncluded);

		foreach (var p in packages)
		{
			var cr = compatibilityUtil.GetPackageData(p);

			if (cr is null)
			{
				//if (p.RequiredPackages is not null)
				//{
				//	foreach (var item in p.RequiredPackages)
				//	{
				//		if (CompatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
				//		{
				//			yield return p;

				//			continue;
				//		}
				//	}
				//}

				continue;
			}

			if (cr.Interactions.ContainsKey(InteractionType.RequiredPackages))
			{
				foreach (var item in cr.Interactions[InteractionType.RequiredPackages].SelectMany(x => x.Interaction.Packages))
				{
					if (compatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
					{
						yield return p;

						continue;
					}
				}
			}

			if (cr.Interactions.ContainsKey(InteractionType.OptionalPackages))
			{
				foreach (var item in cr.Interactions[InteractionType.OptionalPackages].SelectMany(x => x.Interaction.Packages))
				{
					if (compatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
					{
						yield return p;

						continue;
					}
				}
			}
		}
	}
}