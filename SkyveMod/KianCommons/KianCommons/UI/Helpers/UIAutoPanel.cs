namespace KianCommons.UI.Helpers {
    using ColossalFramework.UI;
    using UnityEngine;

    public class UIAutoPanel : UIPanel{
        public void SetSpacing(int x, int y) {
            autoLayoutPadding = new RectOffset(x, x, y, y);
        }
        public override void Awake() {
            base.Awake();
            name = GetType().Name;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutPadding = new RectOffset(3, 3, 3, 3);
            padding = default;
            eventFitChildren += OnAutoFit;

            atlas = TextureUtil.Ingame;
        }

        private void OnAutoFit() {
            if (autoFitChildrenHorizontally) 
                width += padding.right + autoLayoutPadding.right;
            if(autoFitChildrenVertically)
                height += padding.bottom + autoLayoutPadding.bottom;
        }
    }
}
