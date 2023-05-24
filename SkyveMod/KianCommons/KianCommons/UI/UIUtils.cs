namespace KianCommons.UI {
    using System;
    using ColossalFramework.UI;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;

    internal class UIUtils {
        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000038 RID: 56 RVA: 0x00004CF0 File Offset: 0x00002EF0
        public static UIUtils Instance {
            get {
                bool flag = UIUtils.instance == null;
                if (flag) {
                    UIUtils.instance = new UIUtils();
                }
                return UIUtils.instance;
            }
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00004D20 File Offset: 0x00002F20
        private void FindUIRoot() {
            this.uiRoot = null;
            foreach (UIView uiview in UnityEngine.Object.FindObjectsOfType<UIView>()) {
                bool flag = uiview.transform.parent == null && uiview.name == "UIView";
                if (flag) {
                    this.uiRoot = uiview;
                    break;
                }
            }
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00004D84 File Offset: 0x00002F84
        public string GetTransformPath(Transform transform) {
            string text = transform.name;
            Transform parent = transform.parent;
            while (parent != null) {
                text = parent.name + "/" + text;
                parent = parent.parent;
            }
            return text;
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004DD0 File Offset: 0x00002FD0
        public T FindComponent<T>(string name, UIComponent parent = null, UIUtils.FindOptions options = UIUtils.FindOptions.None) where T : UIComponent {
            bool flag = this.uiRoot == null;
            if (flag) {
                this.FindUIRoot();
                bool flag2 = this.uiRoot == null;
                if (flag2) {
                    return default(T);
                }
            }
            foreach (T t in UnityEngine.Object.FindObjectsOfType<T>()) {
                bool flag3 = (options & UIUtils.FindOptions.NameContains) > UIUtils.FindOptions.None;
                bool flag4;
                if (flag3) {
                    flag4 = t.name.Contains(name);
                } else {
                    flag4 = (t.name == name);
                }
                bool flag5 = !flag4;
                if (!flag5) {
                    bool flag6 = parent != null;
                    Transform transform;
                    if (flag6) {
                        transform = parent.transform;
                    } else {
                        transform = this.uiRoot.transform;
                    }
                    Transform parent2 = t.transform.parent;
                    while (parent2 != null && parent2 != transform) {
                        parent2 = parent2.parent;
                    }
                    bool flag7 = parent2 == null;
                    if (!flag7) {
                        return t;
                    }
                }
            }
            return default(T);
        }

        public static IEnumerable<T> GetCompenentsWithName<T>(string name) where T: UIComponent {
            T[] components = GameObject.FindObjectsOfType<T>();
            foreach(T component in components) {
                if (component.name == name)
                    yield return component;
            }
        }

        // Token: 0x04000024 RID: 36
        private static UIUtils instance = null;

        // Token: 0x04000025 RID: 37
        private UIView uiRoot = null;

        // Token: 0x02000010 RID: 16
        [Flags]
        public enum FindOptions {
            // Token: 0x04000034 RID: 52
            None = 0,
            // Token: 0x04000035 RID: 53
            NameContains = 1
        }
    }
}
