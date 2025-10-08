using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    /// <summary>
    /// 텍스트를 렌더링하는 컴포넌트를 나타냅니다.
    /// </summary>
    public class TextRenderer : UIRenderable
    {
        public static readonly bool IsAddableFromInspector = true;

        public static readonly float renderOverSampling = 1f;
        public static readonly float renderFontMultiplier = 1.25f;
        private readonly eDynamicAtlasCategory aaCaterogy = eDynamicAtlasCategory.Font;

        [SerializeField] public eHAlign HAlign = eHAlign.Left;
        [SerializeField] public eVAlign VAlign = eVAlign.Bottom;
        [SerializeField] protected int _fontSize = 80;
        [SerializeField] protected string _text = "hello";
        [SerializeField] protected float _sharpness = 0.5f;
        [SerializeField] protected bool _shadow = false;
        [SerializeField] protected Color _shadowColor = Color.Black;
        [SerializeField] protected Vector2 _shadowOffset = new Vector2(1, -1);
        [SerializeField] protected bool _outline = false;
        [SerializeField] protected Color _outlineColor = Color.Black;
        [SerializeField] protected float _outlineOffset = 1;

        private bool _renderAgain = true;
        private SkiaFontUtility _font = null;
        private Rectangle _atlasRect;
        private SharedMaterial.Reference _material;
        private int _atlasId;
        private RectPrimitiveShard _rectPrimitiveShard = null;
        private int _yOffsetMin = 0;


        public void SetShadow(bool shadow, Color? color = null, Vector2? shadowOffset = null)
        {
            shadowOffset ??= new Vector2(2, -2);
            color ??= Color.Black;

            _shadow = shadow;
            _shadowColor = color.Value;
            _shadowOffset = shadowOffset.Value;
        }

        public void SetOutline(bool outline, Color? color = null, float outlineOffset = 2f)
        {
            color ??= Color.Black;

            _outline = outline;
            _outlineColor = color.Value;
            _outlineOffset = outlineOffset;
        }

        public Vector2 sourceTexSize => _atlasRect.Size.ToVector2();
        public Vector2 presentSize => _size;

        /// <summary>
        /// (test중) 텍스트의 날카로움을 가져오거나 설정합니다.
        /// </summary>
        public float sharpness
        {
            get => _sharpness;
            set
            {
                _sharpness = value;
                _material?.Release();
                _material = null;
            }
        }

        /// <summary>
        /// 텍스트의 폰트 크기를 가져오거나 설정합니다.
        /// </summary>
        public int fontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _renderAgain = true;
            }
        }

        /// <summary>
        /// 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string text
        {
            get => _text;
            set
            {
                _text = value;
                _renderAgain = true;
            }
        }

        /// <summary>
        /// Create a new instance of TextRenderer
        /// </summary>
        public TextRenderer()
        {
            _font = GameBase.Instance.fontManager.GetFont("notoKR");
            _rectPrimitiveShard = new RectPrimitiveShard();
            _atlasId = -1;
        }

        private void RenderString()
        {
            _renderAgain = false;

            if (_atlasId != -1)
            {
                atlasMan.Remove(aaCaterogy, _atlasId);
                _atlasId = -1;
            }

            if (!_text.IsNullOrEmpty())
            {
                var texture = _font.RenderString(_text, fontSize * renderFontMultiplier * renderOverSampling, out _yOffsetMin);

                texture.Name = $"*RenderedString*({fontSize}){_text}";

                _atlasId = atlasMan.Insert(aaCaterogy, texture, false, out _atlasRect);

                texture.Dispose();
            }

            // 갱신을 위해 무효화
            _material?.Release();
            _material = null;
        }

        /// <summary>
        /// 렌더링을 수행합니다.
        /// </summary>
        /// <param name="renderState"></param>
        public override void Render(RenderState renderState)
        {
            if (_renderAgain)
            {
                RenderString();
            }

            if (_atlasId == -1)
            {
                return;
            }

            if (_material == null || atlasMan.IsDirty(aaCaterogy, _atlasId))
            {
                var rect = atlasMan.GetRect(aaCaterogy, _atlasId);
                if (rect.HasValue)
                {
                    _atlasRect = rect.Value;
                }
                else
                {
                    Logger.Log("cannot get rect");
                    return;
                }

                if (_material != null)
                {
                    _material.Release();
                    _material = null;
                }

                var tex = atlasMan.GetTextureByAtlasId(aaCaterogy, _atlasId);
                string address = $"{tex.Name}_{(int)(sharpness * 100f)}_TextRenderer";
                _material = SharedMaterial.Get(address);
                _material.asset.shader = GameBase.Instance.defaultAssets.UI_ttsfontShader;
                _material.asset.SetTexture("_MainTex", tex);
                _material.asset.SetFloat("_Sharpness", sharpness);
                _material.asset.renderPriority = 0;

                _rectPrimitiveShard.sourceRect = _atlasRect;
                _rectPrimitiveShard.textureSize = atlasMan.GetTexSize(aaCaterogy, _atlasId);
            }

            _size = new Vector2(_atlasRect.Width, _atlasRect.Height) / renderOverSampling;

            float w = _atlasRect.Width, h = _atlasRect.Height;
            var px = HAlign switch
            {
                eHAlign.Left => 0,
                eHAlign.Center => .5f,
                eHAlign.Right => 1,
                _ => 0,
            };

            var py = VAlign switch
            {
                eVAlign.Top => 1,
                eVAlign.Middle => .5f,
                eVAlign.Bottom => 0,
                _ => 0,
            };


            renderState.CheckAndAddPrimitiveBatch(_material.asset);
            _rectPrimitiveShard.color = color;
            if (useAsUI)
            {
                var masking = GetComponentInParent<UIMasking>();

                if (masking != null)
                {
                    _rectPrimitiveShard.scissorsID = masking.scissorsID;
                }
                else
                {
                    _rectPrimitiveShard.scissorsID = -1;
                }

                _rectPrimitiveShard.transform = UITransform.canvas.transform;
                _rectPrimitiveShard.transform.position += UITransform.canvasForward * UITransform.accumulatedElevation;

                var ltPos = UITransform.accumulatedRect.Position;
                var margin = (Vector2)UITransform.accumulatedRect.Size - _size;

                _rectPrimitiveShard.offset = ltPos + new Vector2(px, py) * margin;
                _rectPrimitiveShard.size = _size;
                // UI 로 사용할 때에는 상위의 pivot 을 모두 고려해야 하기 때문에, 따로 계산한다.
                // 여기에서는 zero 를 사용
                _rectPrimitiveShard.pivot = Vector2.Zero;
            }
            else
            {
                _rectPrimitiveShard.transform = transform;
                _rectPrimitiveShard.size = _size;
                _rectPrimitiveShard.pivot = new Vector2(px, py);
            }

            if (_outline)
            {
                var offset = _rectPrimitiveShard.offset;
                _rectPrimitiveShard.color = _outlineColor;
                _rectPrimitiveShard.offset = offset + new Vector2(-1, 0) * _outlineOffset;
                _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
                _rectPrimitiveShard.offset = offset + new Vector2(1, 0) * _outlineOffset;
                _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
                _rectPrimitiveShard.offset = offset + new Vector2(0, -1) * _outlineOffset;
                _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
                _rectPrimitiveShard.offset = offset + new Vector2(0, 1) * _outlineOffset;
                _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
                _rectPrimitiveShard.offset = offset;
                _rectPrimitiveShard.color = color;
            }
            else if (_shadow)
            {
                var offset = _rectPrimitiveShard.offset;
                _rectPrimitiveShard.color = _shadowColor;
                _rectPrimitiveShard.offset = offset + _shadowOffset;
                _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
                _rectPrimitiveShard.offset = offset;
                _rectPrimitiveShard.color = color;
            }

            if (_outline || _shadow)
            {
                _rectPrimitiveShard.transform.position += transform.backward * 0.1f;
            }
            _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
        }

        public override void OnDispose()
        {
            base.OnDispose();
            _material?.Release();
            _material = null;
            if (_atlasId != -1)
            {
                atlasMan.Remove(aaCaterogy, _atlasId);
                _atlasId = -1;
            }
        }

        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            _renderAgain = true;
        }

        public Vector2 MeasureSize(string str)
        {
            if (_font == null)
            {
                return Vector2.Zero;
            }

            var size = _font.GetSizeFromString(str, fontSize * renderFontMultiplier * renderOverSampling, out _yOffsetMin);
            return size;
        }

        /// <summary>
        /// TextRenderer 를 생성합니다.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="anchoredRect"></param>
        /// <param name="elevation"></param>
        /// <param name="hAlign"></param>
        /// <param name="vAlign"></param>
        /// <param name="useOutLine"></param>
        /// <param name="useShadow"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static TextRenderer BuildAsUI(Transform parent,
            string name,
            string fontName,
            int fontSize,
            string text,
            Color color,
            RectangleF anchoredRect, float elevation = 0.1f,
            eHAlign hAlign = eHAlign.Left,
            eVAlign vAlign = eVAlign.Bottom,
            bool useOutLine = false,
            bool useShadow = false,
            string layer = "UI")
        {
            var textObj = GameBase.Instance.hierarchyManager.CreateGameObject(name, parent);
            textObj.layer = LayerMask.NameToLayer(layer);
            var textRenderer = textObj.AddComponent<TextRenderer>();
            textRenderer._font = GameBase.Instance.fontManager.GetFont(fontName);
            textRenderer.useAsUI = true;
            textRenderer.text = text;
            textRenderer.fontSize = fontSize;
            textRenderer.color = color;

            textRenderer.HAlign = hAlign;
            textRenderer.VAlign = vAlign;

            textRenderer.SetOutline(useOutLine);
            textRenderer.SetShadow(useShadow);

            var uitransform = textRenderer.UITransform;
            uitransform.elevation = elevation;
            uitransform.anchoredRect = anchoredRect;
            uitransform.pivot = uitransform.anchor = Vector2.Zero;
            return textRenderer;
        }
    }
}
