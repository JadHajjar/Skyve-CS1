using ColossalFramework.UI;

namespace KianCommons.UI.Table {
    internal class UITableCellOuter : UIPanel {

        public UITableCellInner InnerCell;
        public int RowIndex;
        public int ColumnIndex;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoFitChildrenHorizontally = false;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;

            InnerCell = AddUIComponent<UITableCellInner>();
        }
    }
}
