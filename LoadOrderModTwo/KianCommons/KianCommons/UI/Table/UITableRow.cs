using ColossalFramework.UI;
using System;

namespace KianCommons.UI.Table {
    internal class UITableRow : UIPanel {
        internal UITableCellOuter[] Cells = new UITableCellOuter[0];
        internal int RowIndex = -1;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
        }

        internal void Expand(int columnCount) {
            Cells = Cells.Expand(columnCount, (_columnIndex)=> {
                var cell = AddUIComponent<UITableCellOuter>();
                cell.ColumnIndex = _columnIndex;
                cell.RowIndex = RowIndex;
                return cell;
            });
        }

        internal void Shrink(int columnCount) {
            Cells = Cells.Shrink(columnCount, (cell, columnIndex) => {
                Destroy(cell?.gameObject);
            });
        }
    }
}
