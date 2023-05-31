using ColossalFramework.UI;

using KianCommons.Plugins;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;

using static KianCommons.Assertion;

using Object = UnityEngine.Object;

namespace KianCommons.UI;
internal static class TextureUtil
{
	#region atlas
	static UITextureAtlas inGame_;
	static UITextureAtlas inMapEditor_;
	public static UITextureAtlas Ingame
	{
		get
		{
			if (!inGame_)
			{
				inGame_ = GetAtlas("Ingame");
			}

			return inGame_;
		}
	}
	public static UITextureAtlas InMapEditor
	{
		get
		{
			if (!inMapEditor_)
			{
				inMapEditor_ = GetAtlas("InMapEditor");
			}

			return inMapEditor_;
		}
	}

	static string PATH => typeof(TextureUtil).Assembly.GetName().Name + ".Resources.";
	static string ModPath => PluginUtil.GetPlugin().modPath;
	public static string FILE_PATH = ModPath;
	public static bool EmbededResources = true;

	public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, int spriteWidth, int spriteHeight, string[] spriteNames)
	{
		return CreateTextureAtlas(textureFile, atlasName, spriteNames);
	}

	public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, string[] spriteNames)
	{
		Texture2D texture2D;
		if (!EmbededResources)
		{
			texture2D = GetTextureFromFile(textureFile);
		}
		else
		{
			texture2D = GetTextureFromAssemblyManifest(textureFile);
		}

		return CreateTextureAtlas(texture2D, atlasName, spriteNames);
	}


	public static UITextureAtlas CreateTextureAtlas(Texture2D texture2D, string atlasName, string[] spriteNames)
	{
		var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		Assert(atlas, "uitextureAtlas");
		var material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
		Assert(material, "material");
		material.mainTexture = texture2D.TryMakeReadable();
		atlas.material = material;
		atlas.name = atlasName;

		var n = spriteNames.Length;
		for (var i = 0; i < n; i++)
		{
			var num = 1f / spriteNames.Length;
			var spriteInfo = new UITextureAtlas.SpriteInfo
			{
				name = spriteNames[i],
				texture = texture2D,
				region = new Rect(i * num, 0f, num, 1f)
			};
			atlas.AddSprite(spriteInfo);
		}
		return atlas;
	}

	/// <summary>
	/// Create a new atlas.
	/// </summary>
	public static UITextureAtlas CreateTextureAtlas(string atlasName, Texture2D[] textures)
	{
		const int maxSize = 2048;
		var texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
		var regions = texture2D.PackTextures(textures, 2, maxSize);
		var material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
		material.mainTexture = texture2D;

		var textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		textureAtlas.material = material;
		textureAtlas.name = atlasName;

		for (var i = 0; i < textures.Length; i++)
		{
			var item = new UITextureAtlas.SpriteInfo
			{
				name = textures[i].name,
				texture = textures[i],
				region = regions[i],
			};

			textureAtlas.AddSprite(item);
		}

		return textureAtlas;
	}

	public static void AddTexturesToAtlas(UITextureAtlas atlas, Texture2D[] newTextures)
	{
		var textures = new Texture2D[atlas.count + newTextures.Length];

		for (var i = 0; i < atlas.count; i++)
		{
			var texture2D = atlas.sprites[i].texture;
			texture2D = texture2D.TryMakeReadable();
			textures[i] = texture2D;
			textures[i].name = atlas.sprites[i].name;
		}

		for (var i = 0; i < newTextures.Length; i++)
		{
			textures[atlas.count + i] = newTextures[i];
		}

		var regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

		atlas.sprites.Clear();

		for (var i = 0; i < textures.Length; i++)
		{
			var spriteInfo = atlas[textures[i].name];
			atlas.sprites.Add(new UITextureAtlas.SpriteInfo
			{
				texture = textures[i],
				name = textures[i].name,
				border = spriteInfo?.border ?? new RectOffset(),
				region = regions[i],
			});
		}

		atlas.RebuildIndexes();
	}

	public static UITextureAtlas GetAtlas(string name)
	{
		var atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
		for (var i = 0; i < atlases.Length; i++)
		{
			if (atlases[i].name == name)
			{
				return atlases[i];
			}
		}
		return UIView.GetAView().defaultAtlas;
	}
	#endregion

	public static UITextureAtlas GetAtlasOrNull(string name)
	{
		var atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
		return atlases.FirstOrDefault(atlas => atlas.name == name);
	}


	#region loading textures


	public static Stream GetFileStream(string file)
	{
		try
		{
			var path = Path.Combine(FILE_PATH, file);
			return File.OpenRead(path) ?? throw new Exception(path + "not find");
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
	}

	public static Texture2D GetTextureFromFile(string file)
	{
		using var stream = GetFileStream(file);
		return GetTextureFromStream(stream);
	}

	public static Stream GetManifestResourceStream(string file)
	{
		try
		{
			var path = string.Concat(PATH, file);
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(path)
				?? throw new Exception(path + " not find");
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
	}

	public static Texture2D GetTextureFromAssemblyManifest(string file)
	{
		using var stream = GetManifestResourceStream(file);
		return GetTextureFromStream(stream);
	}

	public static Texture2D GetTextureFromStream(Stream stream)
	{
		var texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false);
		texture2D.LoadImage(stream.ReadAllBytes());
		texture2D.wrapMode = TextureWrapMode.Clamp; // for cursor.
		texture2D.Apply(false, false);
		return texture2D;
	}

	static byte[] ReadAllBytes(this Stream stream)
	{
		var array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	#endregion
}
