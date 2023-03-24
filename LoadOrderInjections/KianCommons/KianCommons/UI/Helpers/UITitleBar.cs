namespace KianCommons.UI.Helpers {
    using ColossalFramework.UI;
    using UnityEngine;

    //from SamSamTS
    public class UITitleBar : UIPanel {
        private UILabel m_title;
        private UIButton m_close;
        private UIDragHandle m_drag;

        public bool isModal = false;
        public UIButton closeButton {
            get { return m_close; }
        }

        public string title {
            get { return m_title.text; }
            set {
                if (m_title == null) SetupControls();
                m_title.text = value;
            }
        }

        private void SetupControls() {
            width = parent.width;
            height = 40;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            relativePosition = Vector3.zero;

            m_title = AddUIComponent<UILabel>();
            m_title.autoSize = false;
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.verticalAlignment = UIVerticalAlignment.Middle;
            m_title.text = title;
            m_title.textScale = 1.1f;
            m_title.size = new Vector2(width, height);
            m_title.relativePosition = new Vector3(0, 0);


            m_close = AddUIComponent<UIButton>();
            m_close.atlas = TextureUtil.Ingame;
            m_close.relativePosition = new Vector3(width - 35, 2);
            m_close.normalBgSprite = "buttonclose";
            m_close.hoveredBgSprite = "buttonclosehover";
            m_close.pressedBgSprite = "buttonclosepressed";
            m_close.eventClick += (component, param) => {
                if (isModal && isVisible)
                    UIView.PopModal();
                parent.Hide();
            };

            m_drag = AddUIComponent<UIDragHandle>();
            m_drag.width = width - 50;
            m_drag.height = height;
            m_drag.relativePosition = Vector3.zero;
            m_drag.target = parent;
        }
    }
}
