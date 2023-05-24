namespace KianCommons.UI {
    using System;
    using System.IO;
    using UnityEngine;

    internal static class TextureExtensions {
        /// <summary>
        /// returns a copy of the texture with the difference that: mipmap=false, linear=false, readable=true;
        /// </summary>
        public static Texture2D GetReadableCopy(this Texture2D tex, bool linear = false) {
            Assertion.Assert(tex, "tex!=null");
            Texture2D ret = tex.MakeReadable(linear);
            ret.name = tex.name;
            ret.anisoLevel = tex.anisoLevel;
            ret.filterMode = tex.filterMode;
            return ret;
        }

        public static Texture2D MakeReadable(this Texture texture, bool linear) {
            RenderTextureReadWrite RW_mode = linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RW_mode);
            Graphics.Blit(texture, rt);
            texture = rt.ToTexture2D();
            RenderTexture.ReleaseTemporary(rt);
            return texture as Texture2D;
        }

        public static bool IsReadable(this Texture2D texture) {
            try {
                texture.GetPixel(0, 0);
                return true;
            } catch {
                return false;
            }
        }

        public static Texture2D TryMakeReadable(this Texture2D texture) {
            if (texture.IsReadable())
                return texture;
            else
                return texture.MakeReadable();
        }

        public static void Dump(this Texture2D tex, string path) {
            if (tex == null) throw new ArgumentNullException("tex");
            Log.Called(tex.name, path);
            byte[] bytes = tex.TryMakeReadable().EncodeToPNG() ?? throw new Exception($"bytes == null. Failed to dump {tex?.name} with format  {tex.format} to {path}.");
            File.WriteAllBytes(path, bytes);
            Log.Succeeded();
        }
    }
}
