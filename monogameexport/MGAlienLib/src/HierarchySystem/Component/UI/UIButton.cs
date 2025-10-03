#nullable enable

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
namespace MGAlienLib
{
    public class UIButton : UIImage
    {
        public static new readonly bool IsAddableFromInspector = true;

        [SerializeField] protected bool _useDimmingAnimation = true;
        [SerializeField] protected TextRenderer _text;

        private Color _baseColor;

        public TextRenderer text => _text;

        public override Color color 
        {
            get => base.color;
            set
            {
                base.color = value;
                _baseColor = value;
            }
        }

        public override void Awake()
        {
            base.Awake();
            InitDimmingAnimation();
        }

        public bool useDimminAnimation
        {
            get => _useDimmingAnimation;
            set
            {
                if (_useDimmingAnimation != value)
                {
                    _useDimmingAnimation = value;

                    if (_useDimmingAnimation)
                    {
                        InitDimmingAnimation();
                    }
                    else
                    {
                        StopDimmingAnimation();
                    }
                }
            }
        }

        private void InitDimmingAnimation()
        {
            _baseColor = this.color;
            if (_useDimmingAnimation)
            {
                OnUIPointerEnter -= OnUiPointerEnterDimming;
                OnUIPointerEnter += OnUiPointerEnterDimming;

                OnUIPointerExit -= OnUiPointerExitDimming;
                OnUIPointerExit += OnUiPointerExitDimming;

                OnUIPointerDown -= OnUiPointerDownDimming;
                OnUIPointerDown += OnUiPointerDownDimming;

                OnUIPointerUp -= OnUiPointerUpDimming;
                OnUIPointerUp += OnUiPointerUpDimming;
            }
        }

        private void StopDimmingAnimation()
        {
            this.color = _baseColor;
            OnUIPointerEnter -= OnUiPointerEnterDimming;
            OnUIPointerExit -= OnUiPointerExitDimming;
            OnUIPointerDown -= OnUiPointerDownDimming;
            OnUIPointerUp -= OnUiPointerUpDimming;
        }

        private void OnUiPointerEnterDimming(UIRenderable _) => _color = _baseColor.Dimming(.8f);
        private void OnUiPointerExitDimming(UIRenderable _) => _color = _baseColor;
        private void OnUiPointerDownDimming(UIRenderable _) => _color = _baseColor.Dimming(.5f);
        private void OnUiPointerUpDimming(UIRenderable _) => _color = _baseColor;

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            InitDimmingAnimation();
        }


        public static UIButton Build(Transform parent,
            string name,
            string textureAddress, bool dialate, bool useSlice,
            RectangleF anchoredRect, float z,
            Action<Renderable> onCommand = null,
            string? text = null, Color? textColor = null, string fontName = "notoKR", int fontSize = 12,
            Vector2? pivot = null, Vector2? anchor = null,
            Color? color = null,
            string layer = "UI")
        {
            textColor ??= Color.Black;

            anchor = pivot = Vector2.UnitY;
            var btn = SpriteRenderer.BuildAsUI<UIButton>(parent,
                name,
                textureAddress,
                anchoredRect, z,
                pivot, anchor,
                color,
                layer);

            btn.enableUIRaycast = true;
            btn.OnUICommand += (R) => onCommand?.Invoke(R);

            if (text != null)
            {
                btn._text = TextRenderer.BuildAsUI(btn.transform,
                    name + "_text",
                    fontName, fontSize,
                    text, textColor.Value,
                    new RectangleF(0,0,anchoredRect.Width,anchoredRect.Height), 0.1f,
                    eHAlign.Center, eVAlign.Middle,
                    useOutLine: false,
                    layer: layer);

                btn._text.SetOutline(true, textColor.Value.IsDark()?Color.White:Color.Black);
                btn._text.UITransform.expandWidthToParent = true;
                btn._text.UITransform.expandHeightToParent = true;
            }

            return btn;
        }

    }
}
