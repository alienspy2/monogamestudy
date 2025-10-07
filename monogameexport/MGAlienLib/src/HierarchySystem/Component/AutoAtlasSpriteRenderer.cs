
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 스프라이트를 렌더링하는 컴포넌트를 나타냅니다.
    /// texture 를 load 해서, 자동으로 atlas manager 에 등록하고 렌더링합니다.
    /// 텍스쳐 크기가 그다지 크지 않다면, AutoAtlasSpriteRenderer 를 사용하시기 바랍니다.
    /// 큰 텍스쳐를 사용해야 할 경우, SpriteRenderer 를 사용하시기 바랍니다.
    /// AutoAtlasSpriteRenderer 가 SpriteRenderer 보다 압도적으로 효율적입니다.
    /// </summary>
    public class AutoAtlasSpriteRenderer : UIRenderable
    {
        public static readonly bool IsAddableFromInspector = true;

        private readonly eDynamicAtlasCategory aaCaterogy = eDynamicAtlasCategory.AASprite;

        private PrimitiveShardBase _primitiveShard = null;
        private bool _textureDirty = false;

        private Rectangle _atlasRect;
        private SharedMaterial.Reference _material;
        private int _atlasId;

        [SerializeField(browseFile: true)] protected string _assetAddress;
        [SerializeField] protected bool _flipX = false;
        [SerializeField] protected int _importWidth, _importHeight;
        [SerializeField] protected bool _dialate = false;

        [SerializeField] protected bool _useSlice = false;

        public int sliceLeftMargin
        {
            get
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) return ns.leftMargin;
                else return 0;
            }
            set
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) ns.leftMargin = value;
            }
        }

        public int sliceRightMargin
        {
            get
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) return ns.rightMargin;
                else return 0;
            }
            set
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) ns.rightMargin = value;
            }
        }

        public int sliceTopMargin
        {
            get
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) return ns.topMargin;
                else return 0;
            }
            set
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) ns.topMargin = value;
            }
        }

        public int sliceBottomMargin
        {
            get
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) return ns.bottomMargin;
                else return 0;
            }
            set
            {
                if (_primitiveShard is NineSlicePrimitiveShard ns) ns.bottomMargin = value;
            }
        }

        public bool useSlice
        {
            get => _useSlice;
            set
            {
                if (_useSlice != value)
                {
                    _useSlice = value;
                    MakePrimitiveShard();
                }
            }
        }

        private void MakePrimitiveShard()
        {
            if (_useSlice)
            {
                _primitiveShard = new NineSlicePrimitiveShard();
            }
            else
            {
                _primitiveShard = new RectPrimitiveShard();
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
                if (_flipX != value)
                {
                    _flipX = value;
                    _primitiveShard.Flip = value;
                }
            }
        }

        public AutoAtlasSpriteRenderer()
        {
            _atlasId = -1;
        }


        /// <summary>
        /// 지정된 소스와 주소로 텍스처를 로드합니다.
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="dialate"></param>
        /// <param name="importWidth">override 할 가로 크기. 이 크기로 resize 하여 로드합니다. 실행시간에 resize 하기 때문에, 많이 느립니다. 미리 image 의 크기를 바꿔 두는 것이 훨씬 좋습니다</param>
        /// <param name="importHeight">override 할 세로 크기. 이 크기로 resize 하여 로드합니다 실행시간에 resize 하기 때문에, 많이 느립니다. 미리 image 의 크기를 바꿔 두는 것이 훨씬 좋습니다</param>
        public void Load(string address, bool dialate, int importWidth = 0, int importHeight = 0)
        {
            if (_atlasId != -1)
            {
                atlasMan.Remove(aaCaterogy, _atlasId);
                _atlasId = -1;
            }

            if (_material != null)
            {
                _material.Release();
                _material = null;
            }

            _assetAddress = address;
            _textureDirty = true;
            _dialate = dialate;

            _importWidth = importWidth;
            _importHeight = importHeight;
        }

        /// <summary>
        /// 렌더링을 수행합니다.
        /// </summary>
        /// <param name="renderState"></param>
        public override void Render(RenderState renderState)
        {
            if (_textureDirty)
            {
                UpdateTexture();
            }

            if (_atlasId != -1)
            {
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

                    var tex = atlasMan.GetTextureByAtlasId(aaCaterogy, _atlasId);
                    string address = $"{tex.Name}_todo_AutoAtlasSpriteRenderer";
                    if (_material != null)
                    {
                        _material.Release();
                        _material = null;
                    }

                    _material = SharedMaterial.Get(address);
                    _material.asset.shader = GameBase.Instance.defaultAssets.UI_unlitShader;
                    _material.asset.SetTexture("_MainTex", tex);
                    _material.asset.renderPriority = 0;

                    if (_primitiveShard == null)
                    {
                        MakePrimitiveShard();
                    }

                    _primitiveShard.sourceRect = _atlasRect;
                    _primitiveShard.textureSize = atlasMan.GetTexSize(aaCaterogy, _atlasId);
                }
            }

            if (_material == null) return;

            renderState.CheckAndAddPrimitiveBatch(_material.asset);

            _primitiveShard.color = color;
            _primitiveShard.Flip = _flipX;

            if (useAsUI)
            {
                var masking = GetComponentInParent<UIMasking>();

                if (masking != null)
                {
                    _primitiveShard.scissorsID = masking.scissorsID;
                }
                else
                {
                    _primitiveShard.scissorsID = -1;
                }

                if (UITransform == null) AddComponent<UITransform>();

                _primitiveShard.transform.position = UITransform.canvas.transform.position + UITransform.canvas.transform.forward * UITransform.accumulatedElevation;
                _primitiveShard.transform.rotation = UITransform.canvas.transform.rotation;
                _primitiveShard.transform.scale = UITransform.canvas.transform.scale;

                _primitiveShard.offset = UITransform.accumulatedRect.Position;
                _primitiveShard.size = UITransform.size;
                // UI 로 사용할 때에는 상위의 pivot 을 모두 고려해야 하기 때문에, 따로 계산한다.
                // 여기에서는 zero 를 사용
                _primitiveShard.pivot = Vector2.Zero;
            }
            else
            {
                _primitiveShard.transform = transform;
                _primitiveShard.size = size;
                _primitiveShard.pivot = pivot;
            }

            _primitiveShard.Render(renderState.chunks[_material.asset.id]);
        }

        private void UpdateTexture()
        {
            _textureDirty = false;

            if (_atlasId != -1)
            {
                atlasMan.Remove(aaCaterogy, _atlasId);
                _atlasId = -1;
            }

            var texture = assetManager.GetTexture2D(
                _assetAddress,
                _importWidth, 
                _importHeight);
            if (texture == null)
            {
                Logger.Log($"Cannot load texture : {_assetAddress}");
            }
            else
            {
                _atlasId = atlasMan.Insert(aaCaterogy, texture, _dialate, out _atlasRect);
                texture.Dispose();
            }
        }

        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            Load(_assetAddress, _dialate, _importWidth, _importHeight);
            _textureDirty = true;
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            Load(_assetAddress, _dialate, _importWidth, _importHeight);
        }

        public override void OnDispose()
        {
            base.OnDispose();
            _material?.Release();
            _material = null;
            if (_atlasId != -1)
            {
                atlasMan.Remove(aaCaterogy, _atlasId);
            }
            _atlasId = -1;
        }

        public static T BuildAsUI<T>(Transform parent,
            string name,
            string textureAddress,
            bool dialate,
            bool useSlice = false,
            RectangleF? anchoredRect = null, float? elevation = null,
            Vector2? pivot = null, Vector2? anchor = null,
            Color? color = null,
            string layer = "UI") where T : AutoAtlasSpriteRenderer
        {
            anchoredRect ??= new RectangleF(0, 0, 1, 1);
            elevation ??= 0;
            color ??= Color.White;
            anchor ??= Vector2.UnitY;
            pivot ??= Vector2.UnitY;

            var obj = GameBase.Instance.hierarchyManager.CreateGameObject(name, parent);
            obj.layer = LayerMask.NameToLayer(layer);
            var aaspr = obj.AddComponent<T>();
            aaspr.useAsUI = true;
            aaspr.enableUIRaycast = true;
            aaspr.useSlice = useSlice;
            aaspr.Load(textureAddress, dialate);
            aaspr.size = new Vector2(1, 1);
            aaspr.color = color.Value;

            var uitransform = aaspr.GetComponent<UITransform>();
            uitransform.elevation = elevation.Value;
            uitransform.anchoredRect = anchoredRect.Value;
            uitransform.pivot = pivot.Value;
            uitransform.anchor = anchor.Value;

            return aaspr;
        }
    }
}
