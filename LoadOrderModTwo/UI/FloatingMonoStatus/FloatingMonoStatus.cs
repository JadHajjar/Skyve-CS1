namespace LoadOrderMod.UI{
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;
    using LoadOrderMod.Settings;

    public class FloatingMonoStatus : UILabel {
        public static FloatingMonoStatus Instance { get; private set; }
        public static float SavedX {
            get => ConfigUtil.Config.StatusX;
            set {
                ConfigUtil.Config.StatusX = value;
                ConfigUtil.SaveThread.Dirty = true;
            }
        }

        public static float SavedY {
            get => ConfigUtil.Config.StatusY;
            set {
                if (SavedY != value) {
                    ConfigUtil.Config.StatusY = value;
                    ConfigUtil.SaveThread.Dirty = true;
                }
            }
        }

        private UIDragHandle drag_ { get; set; }

        #region Life Cycle
        public override void Awake() {
            base.Awake();
            textColor = new Color(0.97f, 1f, 0.69f);
            bottomColor = new Color(1f, 0.2f, 0f);
            useGradient = true;
            autoSize = true;
            objectUserData = 1; //refcount
        }

        bool started_ = false;
        public override void Start() {
            LogCalled();
            base.Start();
            Instance = this;
            absolutePosition = new Vector3(SavedX, SavedY);
            tooltip = "controlled by Load Order tool";
            SetupDrag();
            if (Helpers.InStartupMenu) {
                var chirperSprite = UIView.GetAView().FindUIComponent<UISprite>("Chirper");
                chirperSprite.parent.AttachUIComponent(gameObject);
                relativePosition = new Vector3(140, 10);
                drag_.enabled = false;
            }
            started_ = true;
        }

        #endregion

        public void SetupDrag() {
            var dragHandler = new GameObject("UnifiedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            drag_ = dragHandler.AddComponent<UIDragHandle>();

            drag_.width = width;
            drag_.height = height;
            drag_.enabled = true;
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.DebugWait(ThisMethod + " called",
                id: "OnPositionChanged called".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
            if (!started_) return;

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            Vector2 delta = new Vector2(absolutePosition.x - SavedX, absolutePosition.y - SavedY);

            Log.DebugWait(message: "absolutePosition: " + absolutePosition,
                id: "absolutePosition: ".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
        }

        protected override void OnSizeChanged() {
            base.OnSizeChanged();
            if (!started_) return;
            drag_.width = width;
            drag_.height = height;
        }

        protected override void OnMouseUp(UIMouseEventParameter p) {
            base.OnMouseUp(p);
            SavedX = absolutePosition.x;
            SavedY = absolutePosition.y;
        }

        public override string text {
            get => base.text;
            set {
                base.text = value;
                isVisible = !text.IsNullOrWhiteSpace();
            }
        }
    }
}