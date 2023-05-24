using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KianCommons.UI.Table {
    public class UITable : UIPanel {
        private UITableRow[] rows_ = new UITableRow[0];
        private float[] columnWidths_ = new float[0];
        public int RowCount => rows_.Length;
        public int ColumnCount => columnWidths_.Length;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoSize = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
        }

        public void Expand(int rowCount, int columnCount) {
            columnWidths_ = columnWidths_.Expand(columnCount, (_) => -1f);
            rows_ = rows_.Expand(rowCount, (_rowIndex) => {
                var row = AddUIComponent<UITableRow>();
                row.RowIndex = _rowIndex;
                return row;
            });
            foreach (var row in rows_) {
                row.Expand(columnWidths_.Length);
            }
        }
        public void Shrink(int rowCount, int columnCount) {
            var reducedRowCount = rowCount != rows_.Length;
            columnWidths_ = columnWidths_.Shrink(columnCount, (_, _) => { });
            rows_ = rows_.Shrink(rowCount, (_row, _rowIndex) => {
                Destroy(_row?.gameObject);
            });
            foreach (var row in rows_) {
                row.Shrink(columnCount);
            }
            if (reducedRowCount) {
                FitAllColumns();
            }
        }
        internal UITableCellInner GetInnerCell(int row, int column) {
            return GetOuterCell(row, column).InnerCell;
        }
        public UIPanel GetCell(int row, int column) {
            return GetInnerCell(row, column);
        }
        internal UITableCellOuter GetOuterCell(int row, int column) {
            return rows_[row].Cells[column];
        }
        public void FitAllColumns() {
            for (int columnIndex = 0; columnIndex < columnWidths_.Length; columnIndex++) {
                FitColumn(columnIndex);
            }
        }
        public void FitColumn(int columnIndex) {
            float maxWidth = 0;
            for (int rowIndex = 0; rowIndex < rows_.Length; rowIndex++) {
                var innerCell = GetCell(rowIndex, columnIndex);
                if (innerCell.width > maxWidth) {
                    maxWidth = innerCell.width;
                }
            }
            columnWidths_[columnIndex] = maxWidth;
            for (int rowIndex = 0; rowIndex < rows_.Length; rowIndex++) {
                var outerCell = GetOuterCell(rowIndex, columnIndex);
                if (outerCell.width != maxWidth) { //avoid triggering an infinite eventSizeChanged loop
                    outerCell.width = maxWidth;
                }
            }
        }
    }
}
