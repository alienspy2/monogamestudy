using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    /// <summary>
    /// UI 요소의 위치와 크기를 나타내는 컴포넌트입니다.
    /// UI 로 사용하고자 하는 GameObject에 추가합니다.
    /// 
    /// monogame 의 기본 좌표계를 왼쪽 상단이 (0,0) 오른쪽이 +X, 아래쪽이 +Y 입니다.
    /// UITransform 에서는 왼쪽 하단이 (0,0) 오른쪽이 +X, 위쪽이 +Y 입니다.
    /// 
    /// 예를 들어, 화면왼쪽 위에서 100,100 떨어져 있는 경우,
    /// UITransform 의 좌표는 100, -100 이 됩니다.
    /// 이렇게 하는 이유는, 같은 rendable 객체를 2D 와 3D 에서 혼용하기 위함입니다.
    /// </summary>
    public class UITransform : ComponentBase
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField] private RectangleF _anchoredRect = RectangleF.Empty;
        [SerializeField] private Vector2 _pivot = Vector2.Zero;
        [SerializeField] private Vector2 _anchor = Vector2.Zero;
        [SerializeField] private Vector2 _offset = Vector2.Zero;
        [SerializeField] private float _elevation;
        [SerializeField] private bool _expandWidthToParent = false;
        [SerializeField] private float _expandWidthToParentMargin = 0;
        [SerializeField] private bool _expandHeightToParent = false;

        private RectangleF _accumulatedRect;
        private float _accumulatedElevation;
        private bool _isDirty = true;
        private UICanvas _canvas;

        /// <summary>
        /// UI 요소의 위치와 크기를 가져오거나 설정합니다.
        /// 부모에 대해 상대적인 위치와 크기입니다.
        /// </summary>
        public RectangleF anchoredRect
        {
            get { return _anchoredRect; }
            set
            {
                if (_anchoredRect != value)
                {
                    _anchoredRect = value;
                    SetDirty(); // 값이 바뀌면 dirty 플래그 설정
                }
            }
        }


        /// <summary>
        /// UI 요소의 pivot을 가져오거나 설정합니다.
        /// 0,0 은 왼쪽 하단, 1,1은 오른쪽 상단입니다.
        /// </summary>
        public Vector2 pivot
        {
            get => _pivot;
            set
            {
                if (_pivot != value)
                {
                    _pivot = value;
                    SetDirty(); // 값이 바뀌면 dirty 플래그 설정
                }
            }
        }

        /// <summary>
        /// UI 요소의 anchor을 가져오거나 설정합니다.
        /// 부모의 anchoredRect를 기준으로 상대적인 위치입니다.
        /// 0,0 은 부모의 왼쪽 하단, 1,1은 부모의 오른쪽 상단입니다.
        /// pivot 과 anchor 의 조합으로 위치를 결정합니다.
        /// 같은 값을 쓰는 것이 일반적입니다.
        /// </summary>
        public Vector2 anchor
        {
            get => _anchor;
            set
            {
                if (_anchor != value)
                {
                    _anchor = value;
                    SetDirty(); // 값이 바뀌면 dirty 플래그 설정
                }
            }
        }

        /// <summary>
        /// UI 요소의 offset을 가져오거나 설정합니다.
        /// 최종 위치에 더해지는 상대적인 위치입니다.
        /// 필요시에만 사용하세요.
        /// </summary>
        public Vector2 offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    SetDirty(); // 값이 바뀌면 dirty 플래그 설정
                }
            }
        }

        public float elevation
        {
            get => _elevation;
            set
            {
                if (_elevation != value)
                {
                    _elevation = value;
                    SetDirty(); // 값이 바뀌면 dirty 플래그 설정
                }
            }
        }

        public bool expandWidthToParent
        {
            get => _expandWidthToParent;
            set
            {
                _expandWidthToParent = value;
            }
        }

        public float expandWidthToParentMargin
        {
            get => _expandWidthToParentMargin;
            set
            {
                _expandWidthToParentMargin = value;
            }
        }

        public bool expandHeightToParent
        {
            get => _expandHeightToParent;
            set
            {
                _expandHeightToParent = value;
            }
        }



        public Vector2 position
        {
            get => _anchoredRect.Position;
            set
            {
                if (_anchoredRect.Position != value)
                {
                    _anchoredRect.Position = value;
                    SetDirty();
                }
            }
        }

        public Vector2 size
        {
            get => _anchoredRect.Size;
            set
            {
                if ((Vector2)_anchoredRect.Size != value)
                {
                    _anchoredRect.Size = value;
                    SetDirty();
                }
            }
        }

        public UICanvas canvas {
            get
            {
                // todo : canvas 가 변경될 때의 처리 필요
                _canvas ??= transform.GetComponentInParent<UICanvas>();
                return _canvas;
            }
        }

        public Vector3 canvasRight 
        {
            get
            {
                if (canvas == null || canvas.mode == eCanvasType.Screen)
                {
                    return Vector3.UnitX;
                }
                else
                {
                    return canvas.transform.right;
                }
            }
        }

        public Vector3 canvasUp
        {
            get
            {
                if (canvas == null || canvas.mode == eCanvasType.Screen)
                {
                    return Vector3.UnitY;
                }
                else
                {
                    return canvas.transform.up;
                }
            }
        }

        public Vector3 canvasForward
        {
            get
            {
                if (canvas == null || canvas.mode == eCanvasType.Screen)
                {
                    return Vector3.UnitZ;
                }
                else
                {
                    return canvas.transform.forward;
                }
            }
        }

        /// <summary>
        /// 부모 UITransform을 가져옵니다. 부모가 없으면 null을 반환합니다.
        /// </summary>
        public UITransform parent
        {
            get
            {
                if (transform.parent == null)
                {
                    return null;
                }
                return transform.parent.GetComponent<UITransform>();
            }
        }

        /// <summary>
        /// (WIP) 상위 UITransform 의 ancoredRect 를 누적한 UI 요소의 위치와 크기를 가져옵니다.
        /// todo : Z 값이 적용 안되므로, 이렇게 하면 안됨
        /// </summary>
        public RectangleF accumulatedRect
        {
            get
            {
                if (_isDirty || Screen.screenSizeWasChangedThisFrame)
                {
                    // 부모의 worldRect를 가져옴
                    RectangleF parentWorldRect;
                    if (parent != null)
                    {
                        parentWorldRect = parent.accumulatedRect;
                    }
                    else if (_canvas != null)
                    {
                        parentWorldRect = _canvas.rect;
                    }
                    else
                    {
                        parentWorldRect = new RectangleF(0, 0, Screen.width, Screen.height);
                    }

                    // 앵커를 기준으로 한 상대 위치 계산
                    float anchorX = parentWorldRect.X + parentWorldRect.Width * _anchor.X;
                    float anchorY = parentWorldRect.Y + parentWorldRect.Height * _anchor.Y;

                    // anchoredRect와 offset을 더해 최종 위치 계산
                    int x = (int)(anchorX + _anchoredRect.X + _offset.X);
                    int y = (int)(anchorY + _anchoredRect.Y + _offset.Y);

                    // 피봇 오프셋 적용
                    float pivotOffsetX = _anchoredRect.Width * _pivot.X;
                    float pivotOffsetY = _anchoredRect.Height * _pivot.Y;
                    x -= (int)pivotOffsetX;
                    y -= (int)pivotOffsetY;

                    _accumulatedRect = new RectangleF(x, y, _anchoredRect.Width, _anchoredRect.Height); ;

                    _isDirty = false; // 위치 계산 완료
                }


                // 최종 Rectangle 반환
                return _accumulatedRect;
            }
        }

        public float accumulatedElevation
        {
            get
            {
                // todo : cache
                float e = _elevation;
                if (parent != null)
                {
                    e += parent.accumulatedElevation;
                }

                _accumulatedElevation = e;

                return _accumulatedElevation;
            }
        }

        /// <summary>
        ///  강제로 dirty 플래그를 설정합니다.
        ///  다음 프레임에 위치와 크기가 업데이트됩니다.
        /// </summary>
        public void SetDirty()
        {
            _isDirty = true;

            // 자식들을 모두 dirty 플래그 설정
            // todo : 최적화 필요
            foreach (var child in transform.Children)
            {
                child.GetComponent<UITransform>()?.SetDirty();
            }
        }

        public override void OnEnable()
        {
            SetDirty();
            TryGetCanvas();
        }

        /// <summary>
        /// UI 요소의 위치를 화면 안으로 제한합니다.
        /// title bar 가 맨 위에 있다고 가정하기 때문에,
        /// 상단은 창 전체가 제한됩니다.
        /// </summary>
        /// <param name="margin">각 화면 가장자리로부터 여유공간</param>
        public void ClampWindowToBounds(int margin)
        {
            if (anchoredRect.X < -anchoredRect.Width + margin)
            {
                anchoredRect = new RectangleF(-anchoredRect.Width + margin, anchoredRect.Y, anchoredRect.Width, anchoredRect.Height);
            }

            if (anchoredRect.Y > 0)
            {
                anchoredRect = new RectangleF(anchoredRect.X, 0, anchoredRect.Width, anchoredRect.Height);
            }

            if (anchoredRect.X > Screen.width - margin)
            {
                anchoredRect = new RectangleF(Screen.width - margin, anchoredRect.Y, anchoredRect.Width, anchoredRect.Height);
            }

            if (anchoredRect.Y < -Screen.height + margin)
            {
                anchoredRect = new RectangleF(anchoredRect.X, -Screen.height + margin, anchoredRect.Width, anchoredRect.Height);
            }
        }

        public bool IsRayInRect(Ray ray, out float? distance)
        {
            Plane plane = new Plane(Vector3.Zero, Vector3.UnitZ);
            var localRay = new Ray(Vector3.Transform(ray.Position, transform.worldToLocalMatrix), Vector3.TransformNormal(ray.Direction, transform.worldToLocalMatrix));
            var localDistance = localRay.Intersects(plane);

            if (localDistance.HasValue)
            {
                Vector3 localIntersectionPoint = localRay.Position + localRay.Direction * localDistance.Value;

                var worldIntersectionPoint = Vector3.Transform(localIntersectionPoint, transform.localToWorldMatrix);
                worldIntersectionPoint += canvasForward * accumulatedElevation;
                distance = Vector3.Distance(ray.Position, worldIntersectionPoint);

                var rect = this.accumulatedRect;
                return rect.Contains(localIntersectionPoint.ToVector2());
            }

            // Ray doesn't intersect with the plane
            distance = 0f;
            return false;
        }

        /// <summary>
        /// 렌더링 시 호출됩니다.
        /// 내부적으로만 사용됩니다.
        /// 실제로 render 된 UITransform 을 가진 개체만 RaycastTarget 로 등록합니다.
        /// </summary>
        /// <param name="renderState"></param>
        public void internal_OnRender(RenderState renderState)
        {
            renderState.camera.internal_AddUIForUIRaycastTarget(this);
        }

        public bool TryGetCanvas()
        {
            _canvas = GetComponentInParent<UICanvas>();
            return _canvas != null;
        }

        public override void Update()
        {
            base.Update();

            if (_expandWidthToParent || _expandHeightToParent)
            {
                var newSize = this.size;

                if (_expandWidthToParent)
                {
                    if (parent != null)
                    {
                        newSize.X = parent.size.X - expandWidthToParentMargin;
                    }
                    else
                    {
                        newSize.X = canvas.rect.Width - expandWidthToParentMargin;
                    }
                }

                if (_expandHeightToParent)
                {
                    if (parent != null)
                    {
                        newSize.Y = parent.size.Y;
                    }
                    else
                    {
                        newSize.Y = canvas.rect.Height;
                    }
                }

                this.size = newSize;
            }
        }

        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            SetDirty();
        }
    }
}
