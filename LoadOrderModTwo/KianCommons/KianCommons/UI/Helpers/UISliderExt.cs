namespace KianCommons.UI {
    using ColossalFramework.UI;
    using UnityEngine;

    public class UISliderExt : UISlider {
        #region work around the bug with quantizing negative value
        public new float m_StepSize;
        public new float stepSize {
            set {
                base.stepSize = 0f; // work around quantize negative problem.
                this.m_StepSize = value;
            }
            get => this.m_StepSize;
        }

        protected override void OnValueChanged() {
            // fix step size here.
            RoundRawValue(stepSize);
            base.OnValueChanged();
        }

        public static float Round(float val, float step) {
            if (step == 0)return val;
            return Mathf.Round(val / step) * step;
        }

        public void RoundValue(float _stepSize) {
            value = Round(value, _stepSize);
        }

        public void RoundRawValue(float _stepSize) {
            m_RawValue = Round(m_RawValue, _stepSize);
        }

        #endregion

        public override void Awake() {
            base.Awake();
            maxValue = 100;
            minValue = 0;
            stepSize = 0.5f;
            base.stepSize = stepSize * 0.0001f; // work around quantize negative problem.
            scrollWheelAmount = 1f;
            atlas = TextureUtil.Ingame;
        }

        public float Padding = 0; // modify to add padding
        public UISlicedSprite SlicedSprite;

        public override void Start() {
            base.Start();

            builtinKeyNavigation = true;
            isInteractive = true;
            color = Color.grey;
            name = GetType().Name;
            height = 15f;
            width = parent.width - 2 * Padding;
            if(parent is UIPanel parentPanel) {
                width -= parentPanel.padding.horizontal + parentPanel.autoLayoutPadding.horizontal;
            }
            AlignTo(parent, UIAlignAnchor.TopLeft);

            //Log.Debug("parent:" + parent);
            SlicedSprite = AddUIComponent<UISlicedSprite>();
            SlicedSprite.atlas = TextureUtil.Ingame;
            SlicedSprite.spriteName = "ScrollbarTrack";
            SlicedSprite.height = 12;
            SlicedSprite.width = width;
            SlicedSprite.relativePosition = new Vector3(Padding, 2f);

            UISprite thumbSprite = AddUIComponent<UISprite>();
            thumbSprite.atlas = TextureUtil.Ingame;
            thumbSprite.spriteName = "ScrollbarThumb";
            thumbSprite.height = 20f;
            thumbSprite.width = 7f;
            thumbObject = thumbSprite;
            thumbOffset = new Vector2(Padding, 0);
        }

        protected override void OnSizeChanged() {
            base.OnSizeChanged();
            if (SlicedSprite == null || SlicedSprite.parent == null)
                return;
            SlicedSprite.width = SlicedSprite.parent.width - 2 * Padding;
        }


        public static Color MixedValuesColor = Color.Lerp(Color.yellow, Color.white, 0.7f);
        private bool _mixedValues = false;
        public virtual bool MixedValues {
            set {
                if (_mixedValues != value) {
                    _mixedValues = value;
                    if (!value) {
                        thumbObject.color = Color.white;
                        thumbObject.opacity = 1;
                    } else {
                        thumbObject.color = MixedValuesColor;
                        thumbObject.opacity = 0.9f;
                    }
                }
            }
            get => _mixedValues;
        }
    }
}
