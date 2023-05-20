using ColossalFramework.UI;
using UnityEngine;

namespace KianCommons.UI {
    public class UIAutoSizePanel :UIPanel{
        public override void Awake() {
            base.Awake();

            // default values can be overridden.
            autoLayout = true;
            autoSize = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            name = "UIAutoSizePanel";
            atlas = TextureUtil.Ingame;
        }

        public override void Start() {
            base.Start();
            RefreshSizeRecursive();
        }

        private bool m_AutoSize2 = false;

        /// <summary>auto size in the direction other than the autosize direction</summary>
        public bool AutoSize2 {
            get {
                return m_AutoSize2;
            }
            set {
                m_AutoSize2 = value;
                Invalidate();
                RefreshSizeRecursive();
            }
        }

        public void RefreshSizeRecursive() {
            foreach (UIComponent item in components) {
                (item as UIAutoSizePanel)?.RefreshSizeRecursive();
            }
            RefreshSize();
        }

        private void RefreshSize() {
            // copy pasted from UIPanel.AutoArrage().
            float widthAcc = 0f;
            if (this.autoLayoutStart.StartsAtLeft()) {
                widthAcc = (float)this.padding.left + (float)this.autoLayoutPadding.left;
            } else if (this.autoLayoutStart.StartsAtRight()) {
                widthAcc = base.size.x - (float)this.padding.right - (float)this.autoLayoutPadding.right;
            }
            float heightAcc = 0f;
            if (!this.useCenter) {
                if (this.autoLayoutStart.StartsAtTop()) {
                    heightAcc = (float)this.padding.top + (float)this.autoLayoutPadding.top;
                } else if (this.autoLayoutStart.StartsAtBottom()) {
                    heightAcc = base.height + (float)this.padding.bottom + (float)this.autoLayoutPadding.bottom;
                }
            }
            float maxWidth = 0f;
            float maxHeight = 0f;
            for (int i = 0; i < base.childCount; i++) {
                UIComponent uicomponent = null;
                if (this.autoLayoutStart.StartsAtLeft()) {
                    uicomponent = this.m_ChildComponents[i];
                } else if (this.autoLayoutStart.StartsAtRight()) {
                    uicomponent = this.m_ChildComponents[base.childCount - 1 - i];
                }
                //Log.Debug($"{name}.uicomponent={uicomponent.name}:" +
                //    $"{uicomponent.isVisible} && {uicomponent.enabled} && {uicomponent.gameObject.activeSelf}");
                if (uicomponent.isVisible && uicomponent.enabled && uicomponent.gameObject.activeSelf) {
                    //if (!this.useCenter && this.wrapLayout) {
                    //    if (this.autoLayoutDirection == LayoutDirection.Horizontal) {
                    //        if (widthAcc + uicomponent.width >= base.size.x - (float)this.padding.right) {
                    //            if (this.autoLayoutStart.StartsAtLeft()) {
                    //                widthAcc = (float)this.padding.left + (float)this.autoLayoutPadding.left;
                    //            } else if (this.autoLayoutStart.StartsAtRight()) {
                    //                widthAcc = base.size.x - (float)this.padding.right - (float)this.autoLayoutPadding.right;
                    //            }
                    //            if (this.autoLayoutStart.StartsAtTop()) {
                    //                heightAcc += maxHeight;
                    //            } else if (this.autoLayoutStart.StartsAtBottom()) {
                    //                heightAcc -= maxHeight;
                    //            }
                    //            maxHeight = 0f;
                    //        }
                    //    } else if (heightAcc + uicomponent.height + (float)this.autoLayoutPadding.vertical >= base.size.y - (float)this.padding.bottom) {
                    //        if (this.autoLayoutStart.StartsAtTop()) {
                    //            heightAcc = (float)this.padding.top + (float)this.autoLayoutPadding.top;
                    //        } else if (this.autoLayoutStart.StartsAtBottom()) {
                    //            heightAcc = base.height + (float)this.padding.bottom + (float)this.autoLayoutPadding.bottom;
                    //        }
                    //        if (this.autoLayoutStart.StartsAtLeft()) {
                    //            widthAcc += maxWidth;
                    //        } else if (this.autoLayoutStart.StartsAtRight()) {
                    //            widthAcc -= maxWidth;
                    //        }
                    //        maxWidth = 0f;
                    //    }
                    //}
                    //Vector2 pos = Vector2.zero;
                    //if (this.autoLayoutStart.StartsAtLeft()) {
                    //    if (this.useCenter) {
                    //        pos = new Vector2(widthAcc, uicomponent.relativePosition.y);
                    //    } else if (this.autoLayoutStart.StartsAtTop()) {
                    //        pos = new Vector2(widthAcc, heightAcc);
                    //    } else if (this.autoLayoutStart.StartsAtBottom()) {
                    //        pos = new Vector2(widthAcc, heightAcc - uicomponent.height);
                    //    }
                    //} else if (this.autoLayoutStart.StartsAtRight()) {
                    //    if (this.useCenter) {
                    //        pos = new Vector2(widthAcc - uicomponent.width, uicomponent.relativePosition.y);
                    //    } else if (this.autoLayoutStart.StartsAtTop()) {
                    //        pos = new Vector2(widthAcc - uicomponent.width, heightAcc);
                    //    } else if (this.autoLayoutStart.StartsAtBottom()) {
                    //        pos = new Vector2(widthAcc - uicomponent.width, heightAcc - uicomponent.height);
                    //    }
                    //}
                    float currrentWidth = uicomponent.width + (float)this.autoLayoutPadding.horizontal;
                    float currentHeight = uicomponent.height + (float)this.autoLayoutPadding.vertical;
                    maxWidth = Mathf.Max(currrentWidth, maxWidth);
                    maxHeight = Mathf.Max(currentHeight, maxHeight);
                    if (this.autoLayoutDirection == LayoutDirection.Horizontal) {
                        if (this.autoLayoutStart.StartsAtLeft()) {
                            widthAcc += currrentWidth;
                        } else if (this.autoLayoutStart.StartsAtRight()) {
                            widthAcc -= currrentWidth;
                        }
                    } else if (this.autoLayoutStart.StartsAtTop()) {
                        heightAcc += currentHeight;
                    } else if (this.autoLayoutStart.StartsAtBottom()) {
                        heightAcc -= currentHeight;
                    }
                }
            }

            // Appended code:
            if(autoLayoutDirection == LayoutDirection.Horizontal) {
                if (autoLayoutStart.StartsAtLeft())
                    widthAcc += padding.right;
                else
                    widthAcc -= padding.left;
                width = widthAcc;
                if(AutoSize2)
                    height = padding.vertical + maxHeight;
            } else {
                if (autoLayoutStart.StartsAtTop())
                    heightAcc += padding.bottom;
                else
                    heightAcc -= padding.top;
                height = heightAcc;
                if (AutoSize2)
                    width = padding.horizontal + maxWidth;
                //Log.Debug($"updating {name} height to " + height);
            }
        }
    }
}
