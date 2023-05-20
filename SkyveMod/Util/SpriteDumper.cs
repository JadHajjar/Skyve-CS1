using ColossalFramework.IO;

using KianCommons;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyveMod.Util;
internal class SpriteDumper
{
	public static void Dump()
	{
		var sw = Stopwatch.StartNew();

		Log.Info("Dumping Asset Icons...");

		var spritePathBase = Path.Combine(Path.Combine(DataLocation.localApplicationData, "Skyve"), "AssetPictures");

		Directory.CreateDirectory(spritePathBase);

		Dump<PropInfo>(spritePathBase);
		Dump<TreeInfo>(spritePathBase);
		Dump<BuildingInfo>(spritePathBase);
		Dump<NetInfo>(spritePathBase);

		Log.Info($"Dump completed successfully. duration = {sw.ElapsedMilliseconds:#,0}ms", copyToGameLog: true);
	}

	private static void Dump<PrefabType>(string spritePathBase) where PrefabType : PrefabInfo
	{
		var prefabs = PrefabCollection<PrefabType>.LoadedCount();

		Log.Info($"Dumping {typeof(PrefabType).Name} ({prefabs} total)");

		for (uint i = 0; i < prefabs; i++)
		{
			try
			{
				var asset = PrefabCollection<PrefabType>.GetPrefab(i);

				if (asset?.m_Atlas == null)
				{
					continue;
				}

				var assetName = asset.name;
				string fileName;

				if (!assetName.EndsWith("_Data"))
				{
					continue;
				}

				assetName = assetName.Substring(0, assetName.LastIndexOf("_Data")).Replace("  ", " ").Replace(' ', '_').Trim();

				if (Regex.IsMatch(assetName, @"^\d{8,20}\."))
				{
					var folder = Path.Combine(spritePathBase, assetName.Substring(0, assetName.IndexOf('.')));

					Directory.CreateDirectory(folder);

					fileName = Path.Combine(folder, $"{assetName.Substring(assetName.IndexOf('.') + 1).Trim()}.png");
				}
				else
				{
					fileName = Path.Combine(spritePathBase, $"{assetName.Substring(assetName.IndexOf('.') + 1).Trim()}.png");
				}

				if (File.Exists(fileName))
				{
					continue;
				}

				foreach (var sprite in asset.m_Atlas.sprites)
				{
					if (sprite?.texture != null)
					{
						var pngbytes = sprite.texture.EncodeToPNG();

						File.WriteAllBytes(fileName + sprite.name+".png", pngbytes);
					}
				}

				
			}
			catch { }
		}

		Log.Info($"Finished Dumping {typeof(PrefabType).Name}");
	}
}
