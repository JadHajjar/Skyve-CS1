namespace KianCommons.StockCode {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using ColossalFramework;
    using ColossalFramework.Importers;
    using ColossalFramework.IO;
    using ColossalFramework.Threading;
    using ColossalFramework.UI;
    using UnityEngine;

    public class AssetImporterTextureLoader {
        static readonly string sLODModelSignature = "_lod";

        public enum SourceType {
            DIFFUSE,
            NORMAL,
            SPECULAR,
            ALPHA,
            COLOR,
            ILLUMINATION,
            PAVEMENT,
            ROAD,
            COUNT
        }

        protected static readonly string[] SourceTypeSignatures = new string[8] { "_d", "_n", "_s", "_a", "_c", "_i", "_p", "_r" };


        public enum ResultType {
            RGB,
            XYS,
            ACI,
            XYCA,
            APR
        }

        protected static readonly string[] ResultSamplers = new string[5] { "_MainTex", "_XYSMap", "_ACIMap", "_XYCAMap", "_APRMap" };

        protected static readonly Color[] ResultDefaults = new Color[5]
        {
                new Color(1f, 1f, 1f, 1f),
                new Color(0.5f, 0.5f, 0f, 1f),
                new Color(1f, 1f, 0f, 1f),
                new Color(0.5f, 0.5f, 1f, 1f),
                new Color(1f, 0f, 0f, 0f)
        };

        protected static readonly Dictionary<string, bool> ResultLinear = new Dictionary<string, bool>
        {
                { "_MainTex", false },
                { "_XYSMap", true },
                { "_ACIMap", true },
                { "_XYCAMap", true },
                { "_APRMap", true }
        };

        protected static readonly List<SourceType>[] NeededSources = new List<SourceType>[5]
        {
                new List<SourceType> { SourceType.DIFFUSE },
                new List<SourceType>
                {
                        SourceType.NORMAL,
                        SourceType.SPECULAR
                },
                new List<SourceType>
                {
                        SourceType.ALPHA,
                        SourceType.COLOR,
                        SourceType.ILLUMINATION
                },
                new List<SourceType>
                {
                        SourceType.NORMAL,
                        SourceType.COLOR,
                        SourceType.ALPHA
                },
                new List<SourceType>
                {
                        SourceType.ALPHA,
                        SourceType.PAVEMENT,
                        SourceType.ROAD
                }
        };

        protected static readonly string[] SourceTextureExtensions = Image.GetExtensions(Image.SupportedFileFormat.Default);

        protected static TaskDistributor sTaskDistributor = new TaskDistributor("SubTaskDistributor");

        /// <summary>
        /// set modelLoad and model to null to avoid model display all together.
        /// </summary>
        /// <param name="modelLoad">task to create model. to use this set model to null</param>
        /// <param name="model">to use this set modeLoad to null</param>
        /// <param name="results">texture types to load</param>
        /// <param name="path">base path</param>
        /// <param name="modelName">model prefix</param>
        /// <param name="lod">lod textures maybe absent.</param>
        /// <param name="anisoLevel"></param>
        /// <param name="generateDummy">generate empty textures to fill in absent textures</param>
        /// <param name="generatePadding">add 12.5% padding on each side (eg: 128 => l:16,r:16,t:16,b:16 padding) </param>
        /// <returns></returns>
        public static Task LoadTextures(Task<GameObject> modelLoad, GameObject model, ResultType[] results, string path, string modelName, bool lod, int anisoLevel = 1, bool generateDummy = false, bool generatePadding = false) {
            CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "***Creating Texture processing Thread  [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
            Task resultTask = ThreadHelper.taskDistributor.Dispatch(delegate {
                try {
                    CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "******Loading Textures [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                    string baseName = Path.Combine(path, Path.GetFileNameWithoutExtension(modelName));
                    if (lod) baseName += sLODModelSignature;
                    bool[] neededSourceTypes = new bool[(int)SourceType.COUNT];
                    for (int resultIndex = 0; resultIndex < results.Length; resultIndex++) {
                        int index = (int)results[resultIndex];
                        for (int neededSourceIndex = 0; neededSourceIndex < NeededSources[index].Count; neededSourceIndex++) {
                            neededSourceTypes[(int)NeededSources[index][neededSourceIndex]] = true;
                        }
                    }

                    Task<Image>[] readImageTasks = new Task<Image>[neededSourceTypes.Length];
                    for (int k = 0; k < neededSourceTypes.Length; k++) {
                        if (neededSourceTypes[k]) {
                            readImageTasks[k] = LoadTexture((SourceType)k, baseName);
                        }
                    }
                    readImageTasks.WaitAll();
                    if (modelLoad != null) {
                        modelLoad.Wait();
                        model = modelLoad.result;
                    }
                    CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "******Finished loading Textures  [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                    Image[] images = ExtractImages(readImageTasks);

                    // get width/height
                    int width = 0;
                    int height = 0;
                    for (int i = 0; i < images.Length; i++) {
                        if (images[i] != null && images[i].width > 0 && images[i].height > 0) {
                            width = images[i].width;
                            height = images[i].height;
                            break;
                        }
                    }

                    for (int i = 0; i < results.Length; i++) {
                        Color[] texData = BuildTexture(images, results[i], width, height, !lod);
                        ResultType resultType = results[i];
                        string sampler = ResultSamplers[(int)resultType];
                        Color def = ResultDefaults[(int)resultType];
                        bool resultLinear = ResultLinear[sampler];
                        ThreadHelper.dispatcher.Dispatch(delegate {
                            Texture2D texture2D;
                            if (texData != null) {
                                if(sampler != "_XYCAMap") {
                                    texture2D = new Texture2D(width, height, TextureFormat.RGB24, mipmap: false, resultLinear);
                                } else {
                                    texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipmap: false, resultLinear);
                                }
                                texture2D.SetPixels(texData);
                                texture2D.anisoLevel = anisoLevel;
                                texture2D.Apply();
                            } else if (generateDummy && width > 0 && height > 0) {
                                texture2D = new Texture2D(width, height, TextureFormat.RGB24, mipmap: false, ResultLinear[sampler]);
                                if (resultType == ResultType.XYS) {
                                    def.b = 1f - def.b;
                                } else if (resultType == ResultType.APR) {
                                    def.r = 1f - def.r;
                                    def.g = 1f - def.g;
                                }
                                for (int n = 0; n < height; n++) {
                                    for (int num2 = 0; num2 < width; num2++) {
                                        texture2D.SetPixel(num2, n, def);
                                    }
                                }
                                texture2D.anisoLevel = anisoLevel;
                                texture2D.Apply();
                            } else {
                                texture2D = null;
                            }
                            if (generatePadding && texture2D != null) {
                                texture2D = GeneratePaddedTexture(texture2D, 0.125f, resultLinear);
                            }
                            ApplyTexture(model, sampler, texture2D);
                        });
                    }
                    CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "******Finished applying Textures  [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                } catch (Exception ex) {
                    CODebugBase<LogChannel>.Error(LogChannel.AssetImporter, string.Concat(ex.GetType(), " ", ex.Message, " ", ex.StackTrace));
                    UIView.ForwardException(ex);
                }
            });
            CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "***Created Texture processing Thread [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
            return resultTask;
        }

        private static Texture2D GeneratePaddedTexture(Texture2D src, float padding, bool linear) {
            Texture2D texture2D = new Texture2D(src.width, src.height, src.format, mipmap: false, linear);
            texture2D.anisoLevel = src.anisoLevel;
            int width = src.width;
            int height = src.height;
            float ratio = 1f / (1f - 2f * padding);
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    texture2D.SetPixel(j, i, src.GetPixelBilinear(
                        Mathf.Clamp((j / width  - padding) * ratio, 0f, 1f),
                        Mathf.Clamp((i / height - padding) * ratio, 0f, 1f)));
                }
            }
            texture2D.Apply(updateMipmaps: true);
            return texture2D;
        }

        protected static Task<Image> LoadTexture(SourceType source, string baseName) {
            string textureFile = FindTexture(baseName, source);
            if (textureFile == null) {
                CODebugBase<LogChannel>.Warn(LogChannel.AssetImporter, "Texture missing");
                return null;
            }
            Task<Image> task = new Task<Image>((Func<Image>)delegate {
                try {
                    CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "*********Loading texture [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                    Image image = new Image(textureFile);
                    if (image != null && image.width > 0 && image.height > 0) {
                        CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "*********Finished loading texture [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                        return image;
                    }
                    CODebugBase<LogChannel>.Warn(LogChannel.AssetImporter, "Texture: resolution error or texture missing");
                    return null;
                } catch (Exception ex) {
                    CODebugBase<LogChannel>.Error(LogChannel.AssetImporter, string.Concat(ex.GetType(), " ", ex.Message, " ", ex.StackTrace));
                    UIView.ForwardException(ex);
                }
                return null;
            });
            sTaskDistributor.Dispatch(task);
            return task;
        }

        public static string FindTexture(string basePath, SourceType type) {
            for (int i = 0; i < SourceTextureExtensions.Length; i++) {
                string text = basePath + SourceTypeSignatures[(int)type] + SourceTextureExtensions[i];
                if (FileUtils.Exists(text)) {
                    return text;
                }
            }
            return null;
        }

        protected static bool CheckResolutions(int width, int height, params Image[] images) {
            for (int i = 0; i < images.Length; i++) {
                if (images[i] != null && (images[i].width != width || images[i].height != height)) {
                    return false;
                }
            }
            return true;
        }

        protected static Image[] ExtractImages(Task<Image>[] tasks) {
            Image[] array = new Image[tasks.Length];
            for (int i = 0; i < tasks.Length; i++) {
                if (tasks[i] != null) {
                    array[i] = tasks[i].result;
                }
            }
            return array;
        }

        public static void ApplyTexture(GameObject target, string sampler, Texture texture) {
            if (target == null) {
                return;
            }
            Renderer[] componentsInChildren = target.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
            if (componentsInChildren == null || componentsInChildren.Length == 0) {
                componentsInChildren = target.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            }
            for (int i = 0; i < componentsInChildren.Length; i++) {
                for (int j = 0; j < componentsInChildren[i].sharedMaterials.Length; j++) {
                    componentsInChildren[i].sharedMaterials[j].SetTexture(sampler, texture);
                }
            }
        }

        protected static void CombineChannels(Color[] output, Color[] r, Color[] g, Color[] b, Color[] a, bool ralpha, bool galpha, bool balpha, bool rinvert, bool ginvert, bool binvert, bool ainvert, Color defaultColor) {
            for (int i = 0; i < output.Length; i++) {
                float num = ((r == null) ? defaultColor.r : ((!ralpha) ? r[i].r : r[i].a));
                float num2 = ((g == null) ? defaultColor.g : ((!galpha) ? g[i].g : g[i].a));
                float num3 = ((b == null) ? defaultColor.b : ((!balpha) ? b[i].b : b[i].a));
                float num4 = a?[i].a ?? defaultColor.a;
                if (rinvert) {
                    num = 1f - num;
                }
                if (ginvert) {
                    num2 = 1f - num2;
                }
                if (ainvert) {
                    num4 = 1f - num4;
                }
                if (binvert) {
                    num3 = 1f - num3;
                }
                ref Color reference = ref output[i];
                reference = new Color(num, num2, num3, num4);
            }
        }

        protected static Color[] BuildTexture(Image[] sources, ResultType type, int width, int height, bool nullAllowed) {
            List<SourceType> neededSources = NeededSources[(int)type];
            Image[] images = new Image[neededSources.Count];
            for (int i = 0; i < neededSources.Count; i++) {
                images[i] = sources[(int)neededSources[i]];
            }
            if (nullAllowed) {
                bool imageExists = false;
                for (int j = 0; j < images.Length; j++) {
                    if (images[j] != null) {
                        imageExists = true;
                    }
                }
                if (!imageExists) {
                    return null;
                }
            }
            if (CheckResolutions(width, height, images)) {
                Color[] resultColors = new Color[width * height];
                switch (type) {
                    case ResultType.RGB: {
                            Color[] rgb = images[0]?.GetColors();
                            CombineChannels(resultColors, rgb, rgb, rgb, null, ralpha: false, galpha: false, balpha: false, rinvert: false, ginvert: false, binvert: false, ainvert: false, ResultDefaults[(int)type]);
                            break;
                        }
                    case ResultType.XYS: {
                            Color[] n = images[0]?.GetColors(); // normal
                            images[1]?.Convert(TextureFormat.Alpha8);
                            Color[] s = images[1]?.GetColors();
                            CombineChannels(resultColors, n, n, s, null, ralpha: false, galpha: false, balpha: true, rinvert: false, ginvert: false, binvert: true, ainvert: false, ResultDefaults[(int)type]);
                            break;
                        }
                    case ResultType.ACI: {
                            images[0]?.Convert(TextureFormat.Alpha8); //alpha
                            Color[] a = images[0]?.GetColors();
                            images[1]?.Convert(TextureFormat.Alpha8); // color 
                            Color[] c = images[1]?.GetColors();
                            images[2]?.Convert(TextureFormat.Alpha8); // illumination
                            Color[] i = images[2]?.GetColors();
                            CombineChannels(resultColors, a, c, i, null, ralpha: true, galpha: true, balpha: true, rinvert: true, ginvert: true, binvert: false, ainvert: false, ResultDefaults[(int)type]);
                            break;
                        }
                    case ResultType.XYCA: {
                            Color[] n = images[0]?.GetColors(); // normal
                            images[1]?.Convert(TextureFormat.Alpha8);
                            Color[] c = images[1]?.GetColors(); // color
                            images[2]?.Convert(TextureFormat.Alpha8);
                            Color[] a = images[2]?.GetColors(); // alpha
                            CombineChannels(resultColors, n, n, c, a, ralpha: false, galpha: false, balpha: true, rinvert: false, ginvert: false, binvert: true, ainvert: true, ResultDefaults[(int)type]);
                            break;
                        }
                    case ResultType.APR: {
                            images[0]?.Convert(TextureFormat.Alpha8); // alpha
                            Color[] r = images[0]?.GetColors();
                            images[1]?.Convert(TextureFormat.Alpha8); // pavement
                            Color[] g = images[1]?.GetColors();
                            images[2]?.Convert(TextureFormat.Alpha8); // road
                            CombineChannels(resultColors, r, g, images[2]?.GetColors(), null, ralpha: true, galpha: true, balpha: true, rinvert: true, ginvert: true, binvert: false, ainvert: false, ResultDefaults[(int)type]);
                            break;
                        }
                }
                CODebugBase<LogChannel>.VerboseLog(LogChannel.AssetImporter, "******Finished loading & processing textures [" + Thread.CurrentThread.Name + Thread.CurrentThread.ManagedThreadId + "]");
                return resultColors;
            }
            CODebugBase<LogChannel>.Error(LogChannel.AssetImporter, "Texture error: resolutions don't match or textures missing");
            return null;
        }

        public static Task CompressTexture(Material mat, string texName, bool linear) {
            Texture2D texture = mat.GetTexture(texName) as Texture2D;
            if (texture != null) {
                Image img = new Image(texture);
                if (!texName.Equals("_XYCAMap")) {
                    img.Convert(TextureFormat.RGB24);
                }
                return ThreadHelper.taskDistributor.Dispatch(delegate {
                    img.Compress();
                    ThreadHelper.dispatcher.Dispatch(delegate {
                        Texture texture2 = img.CreateTexture(linear);
                        texture2.name = texName;
                        texture2.anisoLevel = texture.anisoLevel;
                        mat.SetTexture(texName, texture2);
                        UnityEngine.Object.Destroy(texture);
                    }).Wait();
                });
            }
            return null;
        }

        private static void BuildDummyLODTextures(GameObject LODObject) {
            if (!(LODObject != null)) {
                return;
            }
            Renderer component = LODObject.GetComponent<Renderer>();
            if (!(component != null) || !(component.sharedMaterial != null)) {
                return;
            }
            Texture texture = component.sharedMaterial.GetTexture("_MainTex");
            Texture texture2 = component.sharedMaterial.GetTexture("_XYSMap");
            Texture texture3 = component.sharedMaterial.GetTexture("_ACIMap");
            int num = 0;
            int num2 = 0;
            if (texture != null) {
                num = texture.width;
                num2 = texture.height;
            } else if (texture2 != null) {
                num = texture2.width;
                num2 = texture2.height;
            } else if (texture3 != null) {
                num = texture3.width;
                num2 = texture3.height;
            }
            Color32[] array = new Color32[num * num2];
            if (texture == null) {
                for (int i = 0; i < num * num2; i++) {
                    ref Color32 reference = ref array[i];
                    reference = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                }
                Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false, linear: false);
                texture2D.SetPixels32(array);
                texture2D.Apply();
                component.sharedMaterial.SetTexture("_MainTex", texture2D);
            }
            if (texture2 == null) {
                for (int j = 0; j < num * num2; j++) {
                    ref Color32 reference2 = ref array[j];
                    reference2 = new Color32(128, 128, byte.MaxValue, byte.MaxValue);
                }
                Texture2D texture2D2 = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false, linear: false);
                texture2D2.SetPixels32(array);
                texture2D2.Apply();
                component.sharedMaterial.SetTexture("_XYSMap", texture2D2);
            }
            if (texture3 == null) {
                for (int k = 0; k < num * num2; k++) {
                    ref Color32 reference3 = ref array[k];
                    reference3 = new Color32(0, 0, 0, byte.MaxValue);
                }
                Texture2D texture2D3 = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false, linear: false);
                texture2D3.SetPixels32(array);
                texture2D3.Apply();
                component.sharedMaterial.SetTexture("_ACIMap", texture2D3);
            }
        }
    }
}