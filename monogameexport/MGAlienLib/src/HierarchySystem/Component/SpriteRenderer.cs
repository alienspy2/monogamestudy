
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 스프라이트를 렌더링하는 컴포넌트를 나타냅니다.
    /// </summary>
    public class SpriteRenderer : UIRenderable
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField(browseFile: true)] protected string _assetAddress;
        [SerializeField] protected bool _flipX = false;
        [SerializeField] protected int _importWidth;
        [SerializeField] protected int _importHeight;

        private SharedTexture.Reference _texRef;
        private RectPrimitiveShard _rectPrimitiveShard = null;
        private SharedMaterial.Reference _material;

        /// <summary>
        /// 스프라이트가 사용하는 텍스처의 소스 영역을 가져오거나 설정합니다. null이면 전체 텍스처를 사용합니다.
        /// </summary>
        public Rectangle? sourceRect
        {
            get
            {
                if (_rectPrimitiveShard == null) return null;
                return _rectPrimitiveShard.sourceRect;
            }
            set
            {
                if (_rectPrimitiveShard == null) return;
                _rectPrimitiveShard.sourceRect = value;
            }
        }

        /// <summary>
        /// 스프라이트의 X축 뒤집기 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool flipX
        {
            get => _flipX;
            set
            {
                _flipX = value;
            }
        }

        ~SpriteRenderer()
        {
            Dispose(false);
        }

        public void Load(string assetAddress, int width = 0, int height = 0)
        {
            Load(assetManager.defaultSource, assetAddress, width, height);
        }

        /// <summary>
        /// 지정된 주소에서 텍스처를 로드합니다.
        /// </summary>
        /// <param name="source">텍스처를 로드할 소스</param>
        /// <param name="assetAddress">로드할 텍스처의 에셋 주소</param>
        /// <param name="width">override 할 가로 크기. 이 크기대로 줄여서 load 한다</param>
        /// <param name="height">override 할 세로 크기. 이 크기대로 줄여서 load 한다</param>
        private void Load(eAssetSource source, string assetAddress, int width = 0, int height = 0)
        {
            if (string.IsNullOrEmpty(assetAddress))
                throw new ArgumentNullException(nameof(assetAddress));

            if (_texRef != null)
            {
                _texRef.Release();
                _texRef = null;
            }
            if (_material != null)
            {
                _material.Release();
                _material = null;
            }

            _texRef = SharedTexture.Get(source, assetAddress, width, height);
            _assetAddress = assetAddress;
            //_width = width;
            //_height = height;
            if (_texRef.asset != null)
            {
                //size = new Vector2(_texRef.asset.Width, _texRef.asset.Height); // 텍스처 로드 시 초기 크기 설정
                //pivot = Vector2.One * .5f;

                _material = SharedMaterial.Get(0, defaultAssets.unlitShader, _texRef);

                _rectPrimitiveShard ??= new RectPrimitiveShard();
                sourceRect = null; // 기본적으로 전체 텍스처 사용
            }
        }

        /// <summary>
        /// 렌더링을 수행합니다.
        /// </summary>
        /// <param name="renderState"></param>
        public override void Render(RenderState renderState)
        {
            if (_material == null) return;
            if (useAsUI && UITransform == null) return;

            renderState.CheckAndAddPrimitiveBatch(_material.asset);

            _rectPrimitiveShard.color = color;
            _rectPrimitiveShard.Flip = flipX;
            _rectPrimitiveShard.textureSize = new Vector2(_texRef.asset.Width, _texRef.asset.Height);

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
                _rectPrimitiveShard.offset = UITransform.accumulatedRect.Position;
                _rectPrimitiveShard.size = UITransform.size;
                // UI 로 사용할 때에는 상위의 pivot 을 모두 고려해야 하기 때문에, 따로 계산한다.
                // 여기에서는 zero 를 사용
                _rectPrimitiveShard.pivot = Vector2.Zero;

            }
            else
            {
                _rectPrimitiveShard.transform = transform;
                _rectPrimitiveShard.size = size;
                _rectPrimitiveShard.pivot = pivot;
            }

            _rectPrimitiveShard.Render(renderState.chunks[_material.asset.id]);
        }

        /// <summary>
        /// 스프라이트가 사용하는 모든 리소스를 해제합니다.
        /// </summary>
        public override void OnDispose()
        {
            base.OnDispose();

            // 관리되는 리소스 해제
            if (_texRef != null)
            {
                _texRef.Release();
                _texRef = null;
            }

            if (_material != null)
            {
                _material.Release();
                _material = null;
            }
        }


        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            if (!_assetAddress.IsNullOrEmpty())
            {
                Load(_assetAddress, _importWidth, _importHeight);
            }
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            if (!_assetAddress.IsNullOrEmpty())
            {
                Load(_assetAddress, _importWidth, _importHeight);
            }
        }

        /// <summary>
        /// SpriteRenderer 를 생성합니다.
        /// Button, Image 용도로 사용합니다.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="textureAddress"></param>
        /// <param name="anchoredRect"></param>
        /// <param name="elevation"></param>
        /// <param name="pivot"></param>
        /// <param name="anchor"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static SpriteRenderer BuildAsUI(Transform parent,
            string name,
            string textureAddress,
            RectangleF anchoredRect, float elevation,
            Vector2? pivot = null, Vector2? anchor = null,
            string layer = "UI")
        {
            var uiimageObj = GameBase.Instance.hierarchyManager.CreateGameObject(name, parent);
            uiimageObj.layer = LayerMask.NameToLayer(layer);
            var spr = uiimageObj.AddComponent<SpriteRenderer>();
            spr.useAsUI = true;
            spr.enableUIRaycast = true;
            spr.Load(textureAddress);
            int w = (int)spr.width;
            int h = (int)spr.height;
            spr.size = new Vector2(1, 1);
            spr.pivot = new Vector2(0, 0);
            spr.color = Color.White;

            var uitransform = uiimageObj.GetComponent<UITransform>();
            uitransform.elevation = elevation;
            uitransform.anchoredRect = anchoredRect;
            uitransform.pivot = pivot.HasValue ? pivot.Value : Vector2.Zero;
            uitransform.anchor = anchor.HasValue ? anchor.Value : Vector2.Zero;

            return spr;
        }
    }
}
