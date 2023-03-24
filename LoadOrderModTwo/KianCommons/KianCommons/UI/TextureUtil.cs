namespace KianCommons.UI {
    using ColossalFramework.UI;
    using System;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Assertion;
    using Object = UnityEngine.Object;
    using Plugins;
    using System.Linq;
    using ColossalFramework.Importers;

    internal static class TextureUtil {
        #region atlas
        static UITextureAtlas inGame_;
        static UITextureAtlas inMapEditor_;
        public static UITextureAtlas Ingame {
            get {
                if (!inGame_)
                    inGame_ = GetAtlas("Ingame");
                return inGame_;
            }
        }
        public static UITextureAtlas InMapEditor {
            get {
                if (!inMapEditor_)
                    inMapEditor_ = GetAtlas("InMapEditor");
                return inMapEditor_;
            }
        }

        static string PATH => typeof(TextureUtil).Assembly.GetName().Name + ".Resources.";
        static string ModPath => PluginUtil.GetPlugin().modPath;
        public static string FILE_PATH = ModPath;
        public static bool EmbededResources = true;

        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, int spriteWidth, int spriteHeight, string[] spriteNames) {
            return CreateTextureAtlas(textureFile, atlasName, spriteNames);
        }

        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, string[] spriteNames) {
            Texture2D texture2D;
            if (!EmbededResources)
                texture2D = GetTextureFromFile(textureFile);
            else
                texture2D = GetTextureFromAssemblyManifest(textureFile);
            return CreateTextureAtlas(texture2D, atlasName, spriteNames);
        }


        public static UITextureAtlas CreateTextureAtlas(Texture2D texture2D, string atlasName, string[] spriteNames) {
            UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Assert(atlas, "uitextureAtlas");
            Material material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            Assert(material, "material");
            material.mainTexture = texture2D.TryMakeReadable();
            atlas.material = material;
            atlas.name = atlasName;

            int n = spriteNames.Length;
            for (int i = 0; i < n; i++) {
                float num = 1f / (float)spriteNames.Length;
                UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo {
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
        public static UITextureAtlas CreateTextureAtlas(string atlasName, Texture2D []textures) {
            const int maxSize = 2048;
            Texture2D texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            Rect[] regions = texture2D.PackTextures(textures, 2, maxSize);
            Material material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;

            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            textureAtlas.material = material;
            textureAtlas.name = atlasName;

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo {
                    name = textures[i].name,
                    texture = textures[i],
                    region = regions[i],
                };

                textureAtlas.AddSprite(item);
            }

            return textureAtlas;
        }

        public static void AddTexturesToAtlas(UITextureAtlas atlas, Texture2D[] newTextures) {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++) {
                Texture2D texture2D = atlas.sprites[i].texture;
                texture2D = texture2D.TryMakeReadable();
                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                    texture = textures[i],
                    name = textures[i].name,
                    border = spriteInfo?.border ?? new RectOffset(),
                    region = regions[i],
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++) {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }
        #endregion

        public static UITextureAtlas GetAtlasOrNull(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            return atlases.FirstOrDefault(atlas => atlas.name == name);
        }


        #region loading textures


        public static Stream GetFileStream(string file) {
            try {
                string path = Path.Combine(FILE_PATH, file);
                return File.OpenRead(path) ?? throw new Exception(path + "not find");
            } catch (Exception ex) {
                Log.Exception(ex);
                throw ex;
            }
        }

        public static Texture2D GetTextureFromFile(string file) {
            using (Stream stream = GetFileStream(file))
                return GetTextureFromStream(stream);
        }

        public static Stream GetManifestResourceStream(string file) {
            try {
                string path = string.Concat(PATH, file);
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(path)
                    ?? throw new Exception(path + " not find");
            } catch (Exception ex) {
                Log.Exception(ex);
                throw ex;
            }
        }

        public static Texture2D GetTextureFromAssemblyManifest(string file) {
            using (Stream stream = GetManifestResourceStream(file))
                return GetTextureFromStream(stream);
        }

        public static Texture2D GetTextureFromStream(Stream stream) {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false);
            texture2D.LoadImage(stream.ReadAllBytes());
            texture2D.wrapMode = TextureWrapMode.Clamp; // for cursor.
            texture2D.Apply(false, false);
            return texture2D;
        }

        static byte[] ReadAllBytes(this Stream stream) {
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            return array;
        }

        #endregion
    }
}
