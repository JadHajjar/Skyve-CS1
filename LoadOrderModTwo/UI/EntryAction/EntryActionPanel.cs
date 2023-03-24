namespace LoadOrderMod.UI.EntryAction {
    extern alias Injections;
    using KianCommons;
    using System;
    using UnityEngine;
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;

    public class EntryActionPanel : UIPanel { 
        protected static readonly Vector3 POSITION = new Vector2(910, 50) - SIZE;
        protected static readonly Vector2 SIZE = new Vector2(160, 40);
        public ActionButton ActionButton => GetComponentInChildren<ActionButton>();
        public override void Awake() {
            try {
                base.Awake();
                anchor = UIAnchorStyle.Top | UIAnchorStyle.Right;
                autoLayoutStart = LayoutStart.TopRight;
                autoLayout = true;
                autoLayoutPadding = new RectOffset(3, 3, 3, 3);
                autoLayoutDirection = LayoutDirection.Horizontal;
                var b = AddUIComponent<WSButton>();
                LogSucceeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public override void Start() {
            base.Start();
            size = SIZE;
            var pos = POSITION;
            if (parent.height < 90)
                pos.y = 10;
            relativePosition = pos;
        }

        public static void UpdateEntry(PackageEntry packageEntry) {
            try {
                Assertion.NotNull(packageEntry, "packageEntry");
                if (packageEntry.asset?.Instantiate<SaveGameMetaData>() is SaveGameMetaData saveGameMetaData) {
                    //Log.Debug("[p2] entry name=" + packageEntry.entryName);
                    GetOrCreateActionPanel(packageEntry).ActionButton.PackageEntry = packageEntry;
                } else {
                    Remove(packageEntry);
                }

                //Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static void Remove(PackageEntry packageEntry) {
            try {
                Assertion.NotNull(packageEntry, "packageEntry");
                Destroy(GetActionPanel(packageEntry)?.gameObject);
                //Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static EntryActionPanel GetOrCreateActionPanel(PackageEntry packageEntry) {
            Assertion.NotNull(packageEntry);
            return GetActionPanel(packageEntry) ?? Create(packageEntry)
                ?? throw new Exception("failed to create panel");
        }

        public static EntryActionPanel GetActionPanel(PackageEntry packageEntry) {
            var ret = packageEntry?.GetComponentInChildren<EntryActionPanel>();
            if (!ret || !ret.enabled) ret = null;
            return ret;//.LogRet(CurrentMethod(1, packageEntry?.entryName));
        }
        

        static EntryActionPanel Create(PackageEntry packageEntry) {
            Log.Called();
            Assertion.Assert(packageEntry, "packageEntry");
            var topPanel = packageEntry.GetComponent<UIPanel>();
            Assertion.Assert(topPanel, "topPanel");
            var ret = topPanel.AddUIComponent<EntryActionPanel>();
            ret.ActionButton.PackageEntry = packageEntry;
            return ret;//.LogRet(ThisMethod);
        }
    }
}
