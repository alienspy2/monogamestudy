using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class UIInputField : UIImage
    {
        public static new readonly bool IsAddableFromInspector = true;
        private static string CaretTexAddress = "raw://art/UI/white.png";

        [SerializeField] protected Vector2 _textOffset = new Vector2(8, 8);
        [SerializeField] protected Color _activatedBGColor = Color.Yellow;
        [SerializeField] protected Color _deactivatedBGColor = Color.White;

        [SerializeField] protected TextRenderer _textRenderer;
        [SerializeField] protected SpriteRenderer _caret;

        private Action<UIInputField> _onEndEdit;
        private Action<UIInputField> _onCancelEdit;
        private Action<UIInputField> _onValueChanged;

        private bool _focus = false;
        private int _cursorIndex = 0;
        private bool _cursorDirty = false;
        private string _oldText = "";

        public Vector2 textOffset
        {
            get => _textOffset;
            set
            {
                _textOffset = value;
                _textRenderer.UITransform.anchoredRect = new RectangleF(_textOffset.X, _textOffset.Y, UITransform.size.X, UITransform.size.Y);
            }
        }

        public Color activatedBGColor
        {
            get => _activatedBGColor;
            set => _activatedBGColor = value;
        }

        public Color deactivatedBGColor
        {
            get => _deactivatedBGColor;
            set => _deactivatedBGColor = value;
        }

        public bool focus => _focus;
        public int cursorIndex
        {
            get => _cursorIndex;
            set
            {
                if (_cursorIndex != value) _cursorDirty = true;

                if (value < 0)
                {
                    _cursorIndex = 0;
                }
                else if (value > _textRenderer.text.Length)
                {
                    _cursorIndex = _textRenderer.text.Length;
                }
                else
                {
                    _cursorIndex = value;
                }
            }
        }

        public TextRenderer textRenderer => _textRenderer;

        public string text
        {
            get => _textRenderer.text;
            set
            {
                _textRenderer.text = value;
                cursorIndex = value.Length;
            }
        }

        public Action<UIInputField> onEndEdit
        {
            get => _onEndEdit;
            set
            {
                _onEndEdit = value;
            }
        }

        public Action<UIInputField> onCancelEdit
        {
            get => _onCancelEdit;
            set
            {
                _onCancelEdit = value;
            }
        }

        public Action<UIInputField> onValueChanged
        {
            get => _onValueChanged;
            set
            {
                _onValueChanged = value;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (_focus)
            {
                inputManager.TryGetFocus(null);
            }
        }

        public override void Awake()
        {
            base.Awake();

            useAsUI = true;
            var textRendererObj = hierarchyManager.CreateGameObject("text", transform);
            textRendererObj.layer = gameObject.layer;
            _textRenderer = textRendererObj.AddComponent<TextRenderer>();
            _textRenderer.useAsUI = true;
            _textRenderer.UITransform.elevation = 0.1f;
            _textRenderer.HAlign = eHAlign.Left;
            _textRenderer.VAlign = eVAlign.Bottom;
            _textRenderer.UITransform.anchoredRect = new RectangleF(_textOffset.X, _textOffset.Y, UITransform.size.X, UITransform.size.Y);

            var caretObj = hierarchyManager.CreateGameObject("caret", transform);
            caretObj.layer = gameObject.layer;
            _caret = caretObj.AddComponent<SpriteRenderer>();
            _caret.useAsUI = true;
            _caret.Load(UIInputField.CaretTexAddress);
            _caret.UITransform.anchoredRect = new RectangleF(0, 0, 3, UITransform.size.Y);
            _caret.UITransform.elevation = 0.1f;
            _caret.UITransform.pivot = new Vector2(0, 1);
            _caret.UITransform.anchor = new Vector2(0, 1);
            _caret.color = Color.Black;
            _caret.enabled = false;
        }

        public override void Start()
        {
            base.Start();
            _caret.UITransform.position = new Vector2(_textOffset.X, 0);
            OnUIPointerDown += (_) =>
            {
                inputManager.TryGetFocus(this);
            };
        }

        public void internal_OnGetFocus()
        {
            color = _activatedBGColor;
            if (cursorIndex > _textRenderer.text.Length)
            {
                cursorIndex = _textRenderer.text.Length;
            }

            _focus = true;
            _oldText = text;
            _caret.enabled = true;
        }

        public void internal_OnLoseFocus()
        {
            color = _deactivatedBGColor;

            text = _oldText;

            _focus = false;
            _caret.enabled = false;
        }

        public void TryGetFocus()
        {
            inputManager.TryGetFocus(this);
        }

        public void internal_OnInput(char c, Keys key)
        {
            if (key == Keys.Back)
            {
                if (_textRenderer.text.Length > 0 && cursorIndex > 0)
                {
                    _textRenderer.text = _textRenderer.text.Remove(cursorIndex - 1, 1);
                    cursorIndex--;
                    onValueChanged?.Invoke(this);
                }
            }
            else if (key == Keys.Enter)
            {
                _oldText = text;
                inputManager.TryGetFocus(null);
                _onEndEdit?.Invoke(this);
            }
            else if (key == Keys.Delete)
            {
                if (_textRenderer.text.Length > 0 && cursorIndex < _textRenderer.text.Length)
                {
                    _textRenderer.text = _textRenderer.text.Remove(cursorIndex, 1);
                    onValueChanged?.Invoke(this);
                }
            }
            else if (key == Keys.Escape)
            {
                inputManager.TryGetFocus(null);
                _onCancelEdit?.Invoke(this);
            }
            else
            {
                _textRenderer.text = _textRenderer.text.Insert(cursorIndex, c.ToString());
                cursorIndex++;
                onValueChanged?.Invoke(this);
            }

        }

        public override void Update()
        {
            base.Update();
            if (_focus)
            {
                if (inputManager.WasPressedThisFrame(Keys.Left))
                {
                    if (cursorIndex > 0)
                    {
                        cursorIndex--;
                    }
                }
                else if (inputManager.WasPressedThisFrame(Keys.Right))
                {
                    if (cursorIndex < _textRenderer.text.Length)
                    {
                        cursorIndex++;
                    }
                }
                else if (inputManager.WasPressedThisFrame(Keys.Home))
                {
                    cursorIndex = 0;
                }
                else if (inputManager.WasPressedThisFrame(Keys.End))
                {
                    cursorIndex = _textRenderer.text.Length;
                }
                // ctrl + v
                else if (inputManager.IsPressed(Keys.LeftControl) && inputManager.WasPressedThisFrame(Keys.V))
                {
                    text = ClipboardHelperSDL.GetClipboardText();
                    cursorIndex = text.Length;
                }
                // ctrl + c
                else if (inputManager.IsPressed(Keys.LeftControl) && inputManager.WasPressedThisFrame(Keys.C))
                {
                    ClipboardHelperSDL.SetClipboardText(text);
                }

                _caret.UITransform.size = new Vector2(3, UITransform.size.Y);
                _caret.color = Color.White.Dimming(Mathf.Sin(Time.time * 10f) * .5f + .5f);
            }

            if (_cursorDirty)
            {
                _cursorDirty = false;
                UpdateCaret();
            }
        }

        private void UpdateCaret()
        {
            if (cursorIndex == 0)
            {
                _caret.UITransform.position = new Vector2(_textOffset.X, 0);
            }
            else
            {
                var size = fontManager.skNotosansKR.GetSizeFromString(
                    _textRenderer.text.Substring(0, cursorIndex),
                    _textRenderer.fontSize * TextRenderer.renderFontMultiplier,
                    out int yOffsetMin);

                _caret.UITransform.position = new Vector2(_textOffset.X + size.X, 0);
            }
        }


        public static UIInputField Build(Transform parent,
            string name,
            string textureAddress, bool dialate, bool useSlice,
            RectangleF anchoredRect, float z, 
            Action<UIInputField> onEndEdit = null,
            string? text = null, string? fontName = null, int? fontSize = null, Color? textColor = null,
            Vector2? pivot = null, Vector2? anchor = null,
            Color? activatedColor = null,
            Color? deactivatedColor = null,
            string layer = "UI")
        {
            fontName ??= "notoKR";
            fontSize ??= 12;
            text ??= "";
            textColor ??= Color.Black;
            activatedColor ??= Color.Yellow;
            deactivatedColor ??= Color.White;
            anchor ??= Vector2.UnitY;
            pivot ??= Vector2.UnitY;

            var inputField = SpriteRenderer.BuildAsUI<UIInputField>(parent,
                name,
                textureAddress,
                anchoredRect, z,
                pivot, anchor,
                deactivatedColor,
                layer);

            inputField.enableUIRaycast = true;
            inputField._activatedBGColor = activatedColor.Value;
            inputField._deactivatedBGColor = deactivatedColor.Value;

            inputField.color = deactivatedColor.Value;
            inputField.name = name + "_text";
            inputField._textRenderer.fontSize = fontSize.Value;
            inputField._textRenderer.color = textColor.Value;
            inputField.text = text;

            inputField._onEndEdit = onEndEdit;

            return inputField;
        }

    }
}
