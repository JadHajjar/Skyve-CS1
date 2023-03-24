namespace LoadOrderMod.UI.EntryStatus {
    extern alias Injections;
    using KianCommons;
    using System;
    using static Injections.LoadOrderInjections.DownloadStatus;
    using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;
    using UnityEngine;
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;
    using HarmonyLib;
    using ColossalFramework.PlatformServices;

    static class Extensions {
        public static bool IsValid(this PublishedFileId id)
            => id != PublishedFileId.invalid && id.AsUInt64 != 0UL;
    }

    public class EntryStatusPanel : UIPanel{
        static readonly Vector3 POSITION = new Vector2(910, 50) - SIZE;
        static readonly Vector2 SIZE = new Vector2(160, 40);
        StatusButton StatusButton => GetComponentInChildren<StatusButton>();
        public override void Awake() {
            try {
                base.Awake();
                anchor = UIAnchorStyle.Top | UIAnchorStyle.Right;
                autoLayoutStart = LayoutStart.TopRight;
                autoLayout = true;
                autoLayoutPadding = new RectOffset(3, 3, 3, 3);
                autoLayoutDirection = LayoutDirection.Horizontal;
                var statusButton = AddUIComponent<StatusButton>();
                LogSucceeded();
            } catch(Exception ex) { ex.Log(); }
        }

        public override void Start() {
            base.Start();
            size = SIZE;
            var pos = POSITION;
            if (parent.height < 90)
                pos.y = 10;
            relativePosition = pos;
        }

        public static void UpdateDownloadStatusSprite(PackageEntry packageEntry) {
            try {
                Assertion.NotNull(packageEntry, "packageEntry");
                var ugc = m_WorkshopDetails(packageEntry);
                if (!packageEntry.publishedFileId.IsValid() || !ugc.publishedFileId.IsValid()) {
                    //Log.Debug("[p0] entry name=" + packageEntry.entryName);
                    RemoveDownloadStatusSprite(packageEntry);
                } else {
                    var status = SteamUtilities.IsUGCUpToDate(ugc, out string reason);
                    if (status == DownloadOK) {
                        //Log.Debug("[p1] entry name=" + packageEntry.entryName);
                        RemoveDownloadStatusSprite(packageEntry);
                    } else {
                        //Log.Debug("[p2] entry name=" + packageEntry.entryName);
                        GetorCreateStatusPanel(packageEntry).StatusButton.SetStatus(status, reason);
                    }
                }
                //Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static void RemoveDownloadStatusSprite(PackageEntry packageEntry) {
            try {
                Assertion.NotNull(packageEntry, "packageEntry");
                Destroy(GetStatusPanel(packageEntry)?.gameObject);
                //Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static EntryStatusPanel GetorCreateStatusPanel(PackageEntry packageEntry) {
            Assertion.NotNull(packageEntry);
            return GetStatusPanel(packageEntry) ?? Create(packageEntry)
                ?? throw new Exception("failed to create panel");
        }

        public static EntryStatusPanel GetStatusPanel(PackageEntry packageEntry) {
            var ret = packageEntry?.GetComponentInChildren<EntryStatusPanel>();
            if (!ret || !ret.enabled) ret = null;
            return ret;//.LogRet(CurrentMethod(1, packageEntry?.entryName));


        }
        

        static EntryStatusPanel Create(PackageEntry packageEntry) {
            Log.Called();
            Assertion.Assert(packageEntry, "packageEntry");
            var topPanel = packageEntry.GetComponent<UIPanel>();
            Assertion.Assert(topPanel, "topPanel");
            var ret = topPanel.AddUIComponent<EntryStatusPanel>();
            ret.StatusButton.UGCDetails = m_WorkshopDetails(packageEntry);
            return ret;//.LogRet(ThisMethod);
        }

        private static AccessTools.FieldRef<PackageEntry, UGCDetails> m_WorkshopDetails =
            AccessTools.FieldRefAccess<PackageEntry, UGCDetails>("m_WorkshopDetails");

    }
}
