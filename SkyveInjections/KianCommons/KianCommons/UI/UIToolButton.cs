using ColossalFramework.UI;
using System;
using UnityEngine;
using System.Reflection;

/* A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad. The sprites are partly copied as well. */

namespace KianCommons.UI {
    using static KianCommons.Assertion;

    internal class UIToolButton : UIButton {
        public static UIToolButton Instace { get; private set;}

        AssemblyName assemblyName => GetType().Assembly.GetName();
        public string AtlasName => assemblyName.Name + "_REV_"+ assemblyName.Version.Revision;
        const int SIZE = 31;
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        public static Vector2 RELATIVE_POSITION = new Vector3(94, 38);

        const string UIToolButtonBg = "UIToolButtonBg";
        const string UIToolButtonBgActive = "UIToolButtonBgFocused";
        const string UIToolButtonBgHovered = "UIToolButtonBgHovered";
        internal const string UIToolIcon = "UIToolIcon";
        internal const string UIToolIconActive = "UIToolIconPressed";

        static UIComponent GetContainingPanel() {
            var ret = UI.UIUtils.Instance.FindComponent<UIComponent>(CONTAINING_PANEL_NAME, null, UI.UIUtils.FindOptions.NameContains);
            Log.Debug("GetPanel returns " + ret);
            return ret ?? throw new Exception("Could not find " + CONTAINING_PANEL_NAME);
        }

        public override void Awake() {
            base.Awake();
            Log.Debug("UIToolButton.Awake() is called." + Environment.StackTrace);
        }

        public override void Start() {
            base.Start();
            Log.Info("UIToolButton.Start() is called.");

            name = "UIToolButton";
            playAudioEvents = true;
            tooltip = "Node Controller";

            var builtinTabstrip = UI.UIUtils.Instance.FindComponent<UITabstrip>("ToolMode", GetContainingPanel(), UI.UIUtils.FindOptions.None);
            AssertNotNull(builtinTabstrip, "builtinTabstrip");

            UIButton tabButton = (UIButton)builtinTabstrip.tabs[0];

            string[] spriteNames = new string[]
            {
                UIToolButtonBg,
                UIToolButtonBgActive,
                UIToolButtonBgHovered,
                UIToolIcon,
                UIToolIconActive
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("sprites.png", AtlasName, SIZE, SIZE, spriteNames);
            }

            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;

            Deactivate();
            hoveredBgSprite = UIToolButtonBgHovered;


            relativePosition = RELATIVE_POSITION;
            size = new Vector2(SIZE, SIZE); 
            Show();
            Log.Info("UIToolButton created successfully.");
            Unfocus();
            Invalidate();
            //if (parent.name == "RoadsOptionPanel(RoadOptions)") {
            //    Destroy(Instance); // destroy old instance after cloning
            //}
            Instace = this;
        }

        public void Activate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = UIToolButtonBgActive;
            normalFgSprite = focusedFgSprite = UIToolIconActive;
            Invalidate();
        }

        public void Deactivate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = UIToolButtonBg;
            normalFgSprite = focusedFgSprite = UIToolIcon;
            Invalidate();
        }

        public override void OnDestroy() {
            base.OnDestroy();
        }

        public override string ToString() => $"UIToolButton:|name={name} parent={parent.name}|";


    }
}
