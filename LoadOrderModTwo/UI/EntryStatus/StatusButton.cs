namespace LoadOrderMod.UI.EntryStatus {
    extern alias Injections;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using Injections::LoadOrderInjections;
    using KianCommons;
    using KianCommons.UI;
    using LoadOrderMod.Util;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class StatusButton : UIButton {
        const string bgSpriteHoveredName = "Hovered";
        const string bgSpritePressedName = "Pressed";

        public string AtlasName => $"{GetType().FullName}_{nameof(StatusButton)}_rev" + typeof(StatusButton).VersionOf();
        public const int SIZE = 40;

        public UGCDetails UGCDetails;

        public override void Awake() {
            try {
                base.Awake();
                isVisible = true;
                size = new Vector2(SIZE, SIZE);
                canFocus = false;
                name = nameof(StatusButton);
                SetupSprites();
                isVisible = false;
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
                    nameof(DownloadStatus.Gone) ,
                    nameof(DownloadStatus.NotDownloaded) ,
                    nameof(DownloadStatus.PartiallyDownloaded),
                    nameof(DownloadStatus.OutOfDate),
                    bgSpriteHoveredName,
                    bgSpritePressedName,
                };
                atlas = atlas_ ??= TextureUtil.CreateTextureAtlas("Resources/Status.png", AtlasName, spriteNames);

                hoveredBgSprite = bgSpriteHoveredName;
                pressedBgSprite = bgSpritePressedName;

            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public void SetStatus(DownloadStatus status, string result) {
            //LogCalled(status, result);
            isVisible = status != DownloadStatus.DownloadOK;
            if (status == DownloadStatus.CatalogOutOfDate) status = DownloadStatus.OutOfDate; // replace sprite
            disabledFgSprite = focusedFgSprite = normalFgSprite = hoveredFgSprite = pressedFgSprite = status.ToString();
            tooltip = result;
            if (!Settings.Tabs.SubscriptionsTab.SteamExePath.IsNullorEmpty())
                tooltip += "\nClick to redownload";
        }

        protected override void OnClick(UIMouseEventParameter p) {
            p.Use();
            try {
                if (UGCDetails.publishedFileId.AsUInt64 != 0 && UGCDetails.publishedFileId != PublishedFileId.invalid) {
                    CheckSubsUtil.Instance.Redownload(UGCDetails.publishedFileId);
                }
            } catch(Exception ex) { ex.Log(); }

            base.OnClick(p);
        }
    }
}