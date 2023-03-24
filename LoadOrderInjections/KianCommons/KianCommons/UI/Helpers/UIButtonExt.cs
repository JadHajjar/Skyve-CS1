namespace KianCommons.UI {
    using ColossalFramework.UI;
    using UnityEngine;

    public class UIButtonExt : UIButton {
        //public static UIButtonExt Instance { get; private set; }
        public bool ParentWith;

        public override void Awake() {
            base.Awake();
            ParentWith = true;
            //Instance = this;
            name = GetType().FullName;

            height = 30f;
            textScale = 0.9f;
            normalBgSprite = "ButtonMenu";
            hoveredBgSprite = "ButtonMenuHovered";
            pressedBgSprite = "ButtonMenuPressed";
            disabledBgSprite = "ButtonMenuDisabled";
            disabledTextColor = new Color32(128, 128, 128, 255);
            autoSize = true;
            canFocus = false;
            textPadding = new RectOffset(5, 5, 5, 5);
            atlas = TextureUtil.Ingame;
        }

        public override void Start() {
            base.Start();
            if (ParentWith) {
                width = parent.width;
                if (parent is UIPanel panel)
                    width -= panel.padding.horizontal;
            }
        }
    }
}


