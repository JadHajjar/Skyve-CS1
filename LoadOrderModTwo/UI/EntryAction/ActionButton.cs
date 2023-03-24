namespace LoadOrderMod.UI.EntryAction {
    extern alias Injections;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework;
    using System.Collections.Generic;
    using System.Linq;

    public class WSButton : ActionButton {
        protected override string FilePath { get; } = "Resources/clipboard.png";

        public override void Awake() {
            base.Awake();
            tooltip = "copy WS item IDs to clipboard\n" +
                "Hold Ctrl for Mods Only. Hold Alt for Assets only";
        }

        protected override void Clicked() {
            try {
                Log.Called();
                if (PackageEntry.asset.Instantiate<SaveGameMetaData>() is SaveGameMetaData saveGameMetaData) {
                    HashSet<string> ids = new();
                    bool bAssets = !Helpers.ControlIsPressed;
                    bool bMods = !Helpers.AltIsPressed;
                    if (!bMods && !bAssets) bMods = bAssets = true;
                    if (bMods) {
                        foreach (var item in saveGameMetaData.mods) {
                            if (item.modWorkshopID != 0 && item.modWorkshopID != PublishedFileId.invalid.AsUInt64) {
                                ids.Add(item.modWorkshopID.ToString());
                            }
                        }
                    }
                    if (bAssets) {
                        foreach (var item in saveGameMetaData.assets) {
                            if (item.modWorkshopID != 0 && item.modWorkshopID != PublishedFileId.invalid.AsUInt64) {
                                ids.Add(item.modWorkshopID.ToString());
                            }
                        }
                    }
                    var text = string.Join(" ", ids.ToArray());
                    Log.Info("copied to clip board: " + text);
                    Clipboard.text = text;
                }

            } catch (Exception ex) {
                ex.Log();
            }
        }
    }
    public abstract class ActionButton : UIButton {
        const string bgSpriteHoveredName = "Hovered";
        const string bgSpritePressedName = "Pressed";
        const string fgName = "Icon";
        protected abstract string FilePath { get; }

        public string AtlasName => $"{GetType().FullName}_{GetType().Name}_rev" + this.VersionOf();
        public const int SIZE = 40;

        public PackageEntry PackageEntry;

        public override void Awake() {
            try {
                base.Awake();
                isVisible = true;
                size = new Vector2(SIZE, SIZE);
                canFocus = false;
                name = GetType().Name;
                SetupSprites();
                isEnabled = true;
            } catch (Exception ex) { Log.Exception(ex); }
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        static UITextureAtlas atlas_;
        public void SetupSprites() {
            TextureUtil.EmbededResources = false;
            try {
                string[] spriteNames = new string[] {
                    fgName,
                    bgSpriteHoveredName,
                    bgSpritePressedName,
                };
                atlas = atlas_ ??= TextureUtil.CreateTextureAtlas(FilePath, AtlasName, spriteNames);

                hoveredBgSprite = bgSpriteHoveredName;
                pressedBgSprite = bgSpritePressedName;
                disabledFgSprite = focusedFgSprite = normalFgSprite = hoveredFgSprite = pressedFgSprite = fgName;
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        protected bool IsValidWSItem => PackageEntry.publishedFileId.AsUInt64 != 0 && PackageEntry.publishedFileId != PublishedFileId.invalid;
        protected override void OnClick(UIMouseEventParameter p) {
            p.Use();
            try {
                Clicked();
            } catch(Exception ex) { ex.Log(); }
            base.OnClick(p);
        }

        protected abstract void Clicked();
    }
}