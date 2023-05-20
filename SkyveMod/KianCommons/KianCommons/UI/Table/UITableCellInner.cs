using ColossalFramework.UI;
using System;
using UnityEngine;

namespace KianCommons.UI.Table {
    internal class UITableCellInner : UIPanel {

        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoSize = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            padding = new RectOffset(4, 4, 4, 4);
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
            eventSizeChanged += (_, _) => {
                // the first size change occurs within UITableCellOuter::Activate, at which point UITableCellOuter.parent isn't a UITableRow yet.
                var _outerCell = parent as UITableCellOuter;
                var _row = _outerCell?.parent as UITableRow;
                var _table = _row?.parent as UITable;
                _table?.FitColumn(_outerCell.ColumnIndex);
            };
        }
    }
}
