using UnityEngine;
using ColossalFramework.UI;

namespace KianCommons.UI {
    /// <summary>
    /// Static utilities class for managing custom tooltips.
    /// </summary>
    internal static class CustomTooltip
    {
        // Custom tooltip box.
        private static UILabel toolTipBox_;


        /// <summary>
        /// Custom tooltip box.
        /// </summary>
        internal static UILabel TooltipBox
        {
            get
            {
                if (toolTipBox_ == null)
                {
                    toolTipBox_ = CustomTooltipBox();
                }

                return toolTipBox_;
            }
        }


        /// <summary>
        /// Creates a custom tooltip box.
        /// </summary>
        /// <returns>New tooltip box</returns>
        private static UILabel CustomTooltipBox()
        {
            // Create GameObject and attach new UILabel.
            GameObject tooltipGameObject = new GameObject("CustomTooltip");
            tooltipGameObject.transform.parent = UIView.Find("DefaultTooltip").gameObject.transform.parent;
            UILabel tipBox = tooltipGameObject.AddComponent<UILabel>();

            // Size.
            tipBox.autoSize = true;
            tipBox.minimumSize = new Vector2(500f, 12f);
            tipBox.wordWrap = true;

            // Mimic game's default tooltip.
            tipBox.padding = new RectOffset(23, 23, 5, 5);
            tipBox.verticalAlignment = UIVerticalAlignment.Middle;
            tipBox.pivot = UIPivotPoint.BottomLeft;
            tipBox.arbitraryPivotOffset = new Vector2(-3, 6);

            // Appearance.
            tipBox.backgroundSprite = "InfoDisplay";

            // Start hidden and off to the side.
            tipBox.transformPosition = new Vector3(-2f, -2f);
            tipBox.isVisible = false;

            return tipBox;
        }
    }
}