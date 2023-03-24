using System;
using ColossalFramework.UI;
using UnityEngine;
using static KianCommons.ReflectionHelpers;
using KianCommons;
using System.Reflection;

namespace KianCommons.UI.Helpers {
    internal static class UIDropDownExtensions {
        static FieldInfo fPopop = GetField(typeof(UIDropDown), "m_Popup");

        internal static UIListBox GetPopup(this UIDropDown dd) => fPopop.GetValue(dd) as UIListBox;

        static FieldInfo fHoverIndex = GetField(typeof(UIListBox), "m_HoverIndex");

        internal static int GetHoverIndex(this UIDropDown dd) {
            var popup = dd.GetPopup();
            if (popup == null || !popup.isVisible)
                return -1;
            return (int)fHoverIndex.GetValue(popup);
        }
    }

    internal class UIDropDownExt : UIDropDown{
        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.GetAtlas("Ingame");
            size = new Vector2(120f, 30);
            listBackground = "GenericPanelLight";
            itemHeight = 25;
            itemHover = "ListItemHover";
            itemHighlight = "ListItemHighlight";
            normalBgSprite = "ButtonMenu";
            disabledBgSprite = "ButtonMenuDisabled";
            hoveredBgSprite = "ButtonMenuHovered";
            focusedBgSprite = "ButtonMenu";
            listWidth = 120;
            listHeight = 700;
            listPosition = UIDropDown.PopupListPosition.Below;
            clampListToScreen = true;
            builtinKeyNavigation = true;
            foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            popupColor = new Color32(45, 52, 61, 255);
            popupTextColor = new Color32(170, 170, 170, 255);
            zOrder = 1;
            textScale = 0.8f;
            verticalAlignment = UIVerticalAlignment.Middle;
            horizontalAlignment = UIHorizontalAlignment.Left;
            selectedIndex = 0;
            textFieldPadding = new RectOffset(8, 0, 8, 0);
            itemPadding = new RectOffset(14, 0, 8, 0);
            //AlignTo(parent, UIAlignAnchor.TopLeft);

            var button = AddUIComponent<UIButton>();
            triggerButton = button;
            atlas = TextureUtil.Ingame;
            button.text = "";
            button.size = size;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;
            button.textScale = 0.8f;
            listWidth = (int)width;
            awakened_ = true;
        }
        bool awakened_ = false;
        protected override void OnSizeChanged() {
            base.OnSizeChanged();
            if (!awakened_) return;
            triggerButton.size = size;
            listWidth = (int)width;
        }

    }
}

    