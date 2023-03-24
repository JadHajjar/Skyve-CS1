namespace KianCommons.UI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    internal static class UIKeyMappingsExtensions {
        internal static UIKeymappingsPanel AddKeymappingsPanel(this UIHelper helper) {
            return (helper.self as UIComponent).gameObject.AddComponent<UIKeymappingsPanel>();
        }
    }

    internal class UIKeymappingsPanel : UICustomControl {
        internal UIComponent AddKeymapping(string label, SavedInputKey savedInputKey) {
            UIPanel uipanel = base.component.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel;
            int num = this.count;
            this.count = num + 1;
            if(num % 2 == 1) {
                uipanel.backgroundSprite = null;
            }
            UILabel uilabel = uipanel.Find<UILabel>("Name");
            UIButton uibutton = uipanel.Find<UIButton>("Binding");
            uibutton.eventKeyDown += this.OnBindingKeyDown;
            uibutton.eventMouseDown += this.OnBindingMouseDown;
            uilabel.text = label;
            uibutton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uibutton.objectUserData = savedInputKey;
            uipanel.eventVisibilityChanged += (_, __) => RefreshBindableInputs();
            return uibutton;
        }


        private static bool IsModifierKey(KeyCode code) {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        private static bool IsControlDown() {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private static bool IsShiftDown() {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private static bool IsAltDown() {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private static bool IsUnbindableMouseButton(UIMouseButton code) {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        private static KeyCode ButtonToKeycode(UIMouseButton button) {
            if(button == UIMouseButton.Left) {
                return KeyCode.Mouse0;
            }
            if(button == UIMouseButton.Right) {
                return KeyCode.Mouse1;
            }
            if(button == UIMouseButton.Middle) {
                return KeyCode.Mouse2;
            }
            if(button == UIMouseButton.Special0) {
                return KeyCode.Mouse3;
            }
            if(button == UIMouseButton.Special1) {
                return KeyCode.Mouse4;
            }
            if(button == UIMouseButton.Special2) {
                return KeyCode.Mouse5;
            }
            if(button == UIMouseButton.Special3) {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        private void RefreshBindableInputs() {
            foreach(UIButton c in GetComponentsInChildren<UIButton>()) {
                if(c.name == "Binding" &&
                    c.objectUserData is SavedInputKey savedInputKey) {
                    c.text = savedInputKey.ToLocalizedString("KEYNAME");
                }
            }
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p) {
            if(this.m_EditingBinding != null && !IsModifierKey(p.keycode)) {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey value = (p.keycode == KeyCode.Escape) ? this.m_EditingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if(p.keycode == KeyCode.Backspace) {
                    value = SavedInputKey.Empty;
                }
                this.m_EditingBinding.value = value;
                (p.source as UITextComponent).text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                this.m_EditingBinding = null;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p) {
            if(this.m_EditingBinding == null) {
                p.Use();
                this.m_EditingBinding = (SavedInputKey)p.source.objectUserData;
                UIButton uibutton = p.source as UIButton;
                uibutton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uibutton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
                return;
            }
            if(!IsUnbindableMouseButton(p.buttons)) {
                p.Use();
                UIView.PopModal();
                InputKey value = SavedInputKey.Encode(ButtonToKeycode(p.buttons), IsControlDown(), IsShiftDown(), IsAltDown());
                this.m_EditingBinding.value = value;
                UIButton uibutton2 = p.source as UIButton;
                uibutton2.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                uibutton2.buttonsMask = UIMouseButton.Left;
                this.m_EditingBinding = null;
            }
        }

        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";

        private SavedInputKey m_EditingBinding;

        private int count;
    }
}
