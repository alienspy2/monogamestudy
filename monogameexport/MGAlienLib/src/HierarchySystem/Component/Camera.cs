using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 카메라 컴포넌트를 나타냅니다.
    /// </summary>
    public class Camera : ComponentBase
    {
        public static readonly bool IsAddableFromInspector = true;

        /// <summary>
        /// 클리어 타입을 나타냅니다.
        /// </summary>
        public enum eCameraClearFlag
        {
            /// <summary>
            /// 클리어하지 않습니다.
            /// </summary>
            Nothing,
            /// <summary>
            /// 컬러로 클리어합니다.
            /// </summary>
            SolidColor,
            /// <summary>
            /// depth buffer 만 클리어합니다.
            /// </summary>
            Depth
        }

        [SerializeField] protected Rectangle _viewport;
        [SerializeField] protected float _nearClipPlane = 1000;
        [SerializeField] protected float _farClipPlane = 10000;
        [SerializeField] protected bool _orthographic = false;
        [SerializeField] protected float _orthographicSize = 1.0f;
        [SerializeField] protected float _fieldOfView = 45f;
        [SerializeField] protected float _aspectRatio = 1f;
        [SerializeField] protected bool _useAsUI = false;
        [SerializeField] protected Transform _uiRoot = null;
        [SerializeField] protected int _cullingMask = 0x7FFFFFFF;
        [SerializeField] protected int _renderPriority = 0;

        private List<UITransform> _UIRaycastTargets;
        private UITransform _lastPickedUITransform = null;
        private List<UIRenderable> _pointerDownRenderablesCache = new List<UIRenderable>();
        private Matrix? _matViewProjection = null;
        private Matrix? _matViewProjectionINV = null;

        public Rectangle viewport { get { return _viewport; } set { _viewport = value; UpdateProjection(); } }
        public float nearClipPlane { get { return _nearClipPlane; } set { _nearClipPlane = value; UpdateProjection(); } }
        public float farClipPlane { get { return _farClipPlane; } set { _farClipPlane = value; UpdateProjection(); } }
        public bool orthographic { get { return _orthographic; } set { _orthographic = value; UpdateProjection(); } }
        public float orthographicSize { get { return _orthographicSize; } set { _orthographicSize = value; UpdateProjection(); } }
        public float fieldOfView { get { return _fieldOfView; } set { _fieldOfView = value; UpdateProjection(); } }
        public float aspectRatio { get { return _aspectRatio; } set { _aspectRatio = value; UpdateProjection(); } }

        public bool useAsUI { get { return _useAsUI; } set { _useAsUI = value; } }
        public Transform uiRoot { get { return _uiRoot; } set { _uiRoot = value; } }

        public int renderPriority
        {
            get => _renderPriority;
            set
            {
                _renderPriority = value;
            }
        }

        [SerializeField] public int cullingMask
        {
            get => _cullingMask;
            set
            {
                _cullingMask = value;
            }
        }

        /// <summary>
        /// 카메라의 위치를 가져옵니다.
        /// </summary>
        public Vector3 pos => transform.position;
        /// <summary>
        /// 카메라의 회전을 가져옵니다.
        /// </summary>
        public Quaternion rot => transform.rotation;

        /// <summary>
        /// 카메라의 클리어 타입을 가져오거나 설정합니다.
        /// </summary>
        public eCameraClearFlag clearFlags = eCameraClearFlag.SolidColor;
        /// <summary>
        /// 카메라의 클리어 색상을 가져오거나 설정합니다.
        /// 클리어 타입이 Color일 때만 사용됩니다.
        /// </summary>
        public Color backgroundColor = Color.CornflowerBlue;

        /// <summary>
        /// world space 에서 view space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix matView => transform.worldToLocalMatrix;
        /// <summary>
        /// view space 에서 projection space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix matProjection { get; private set; } = Matrix.Identity;
        /// <summary>
        /// world space 에서 projection space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix matViewProjection
        {
            get
            {
                // 만들어진 직후에 사용할 수 있도록 처리
                if (_matViewProjection == null)
                    _matViewProjection = matView * matProjection;
                return _matViewProjection.Value;
            }
            private set
            {
                _matViewProjection = value;
            }
        }
        /// <summary>
        /// projection space 에서 world space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix matViewProjectionINV
        {
            get
            {
                // 만들어진 직후에 사용할 수 있도록 처리
                if (_matViewProjectionINV == null)
                    _matViewProjectionINV = Matrix.Invert(matViewProjection);
                return _matViewProjectionINV.Value;
            }
            private set
            {
                _matViewProjectionINV = value;
            }
        }

        private GraphicsDevice device => GameBase.Instance.GraphicsDevice;


        /// <summary>
        /// Create a new instance of Camera
        /// </summary>
        public Camera()
        {
            viewport = new Rectangle(0, 0, Screen.width, Screen.height);
            aspectRatio = (float)viewport.Width / (float)viewport.Height;
        }

        public Vector3 Unproject(Vector2 screenPos, float z_world)
        {
            Vector3 screenSpace = new Vector3(screenPos, 0);
            Vector3 nearPoint = ViewportUnproject(screenSpace, _nearClipPlane);
            Vector3 farPoint = ViewportUnproject(screenSpace, _farClipPlane);

            float t = (z_world - nearPoint.Z) / (farPoint.Z - nearPoint.Z);
            return Vector3.Lerp(nearPoint, farPoint, t);
        }

        public Ray ScreenPointToRay(Vector2 screenPos)
        {
            // screen pos 는 왼쪽 상단이 0,0, 화면 아래쪽이 +Y
            // ui 는 왼쪽 하단이 0,0, 위쪽이 +Y 이므로,
            // ScreenPointToRay를 통해 변환해줘야한다.
            Vector3 nearPoint = ViewportUnproject(new Vector3(screenPos, 0), _nearClipPlane);
            Vector3 farPoint = ViewportUnproject(new Vector3(screenPos, 0), _farClipPlane);
            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            return new Ray(nearPoint, direction);
        }

        private Vector3 ViewportUnproject(Vector3 screenSpace, float depth)
        {
            Vector3 ndc = new Vector3(
                (screenSpace.X / _viewport.Width) * 2f - 1f,
                1f - (screenSpace.Y / _viewport.Height) * 2f,
                (depth - _nearClipPlane) / (_farClipPlane - _nearClipPlane)
            );

            Matrix inverseViewProj = matViewProjectionINV;// Matrix.Invert(_view * _projection);
            Vector4 worldSpace = Vector4.Transform(new Vector4(ndc, 1f), inverseViewProj);
            return new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z) / worldSpace.W;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            GameBase.Instance.renderQueue.AddCamera(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GameBase.Instance.renderQueue.RemoveCamera(this);
        }

        private void UpdateProjection()
        {
            if (orthographic)
            {
                matProjection = Matrix.CreateOrthographicOffCenter(
                                          -orthographicSize * aspectRatio, orthographicSize * aspectRatio,
                                          -orthographicSize, orthographicSize,
                                          nearClipPlane, farClipPlane);
            }
            else
            {
                matProjection = Matrix.CreatePerspectiveFieldOfView(Mathf.Deg2Rad * fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        /// <summary>
        /// 렌더링 준비를 합니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        /// <param name="stage"></param>
        public void internal_GetReadyToRender(int stage)
        {
            matViewProjection = matView * matProjection;
            matViewProjectionINV = Matrix.Invert(matViewProjection);

            device.Viewport = new Viewport(viewport);

            if (stage == 1)
            {
                switch (clearFlags)
                {
                    case Camera.eCameraClearFlag.SolidColor:
                        device.Clear(backgroundColor);
                        break;
                    case Camera.eCameraClearFlag.Depth:
                        device.Clear(ClearOptions.DepthBuffer, backgroundColor, 1.0f, 0);
                        break;
                }
            }

            if (stage == 0)
            {
                if (_useAsUI)
                {
                    if (_UIRaycastTargets == null)
                        _UIRaycastTargets = new List<UITransform>();
                    else
                        _UIRaycastTargets.Clear();
                }
            }
        }

        /// <summary>
        /// 렌더링이 끝났을 때 호출됩니다.
        /// UIRaycast 를 위한 작업을 수행합니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        /// <param name="ui"></param>
        public void internal_AddUIForUIRaycastTarget(UITransform ui)
        {
            if (_useAsUI)
                _UIRaycastTargets.Add(ui);
        }

        /// <summary>
        /// (WIP) 이 카메라에 의해 렌더링 된 UI 중, camera 로부터 가장 가까운 UI 를 찾아 반환합니다.
        /// projection 일 떄에도 동작합니다.
        /// todo : rect 는 AABB 이므로, 이렇게 하면 안됨. UITransform 의 local 좌표계로 변환한 후 계산해야함.
        /// </summary>
        /// <param name="screenPos">화면 좌표. 좌측상단이 0,0. 화면 오른쪽이 +X, 화면 아래쪽이 +Y 입니다. input manager 의 마우스 좌표계와 동일합니다</param>
        /// <returns>찾은 대상. 없다면 null</returns>
        public UITransform UIRaycast(Vector2 screenPos)
        {
            if (!_useAsUI)
                return null;

            _UIRaycastTargets ??= new List<UITransform>();

            // screen pos 는 왼쪽 상단이 0,0, 화면 아래쪽이 +Y
            // ui 는 왼쪽 하단이 0,0, 위쪽이 +Y 이므로,
            // ScreenPointToRay를 통해 변환해줘야한다.
            var ray = ScreenPointToRay(screenPos);

            var pickingPosition = new Vector2(ray.Position.X, ray.Position.Y);

            UITransform closestUI = null;
            float closestDistance = float.MaxValue;
            Vector3 worldPos = transform.position;

            foreach (var ui in _UIRaycastTargets)
            {
                if (ui.gameObject.active == false)
                    continue;

                var masking = ui.GetComponentInParent<UIMasking>();
                if (masking != null && masking.gameObject.Id != ui.gameObject.Id)
                {
                    if (!masking.UITransform.IsRayInRect(ray, out float? _))
                    {
                        continue;
                    }
                }

                if (ui.IsRayInRect(ray, out float? distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance!.Value;
                        closestUI = ui;
                    }
                }
            }

            return closestUI;
        }


        public override void Update()
        {
            if (_useAsUI)
            {
                //return; 
                // debugging 시에는 이 아래 부분을 사용하지 않는게 편하다

                var pickedUITransform = UIRaycast(inputManager.GetMousePos());

                if (_lastPickedUITransform != pickedUITransform)
                {
                    if (_lastPickedUITransform != null)
                    {
                        foreach (var renderable in _lastPickedUITransform.GetComponents<UIRenderable>())
                        {
                            renderable.internal_OnUIPointerExit();
                        }
                    }

                    if (pickedUITransform != null)
                    {
                        foreach (var renderable in pickedUITransform.GetComponents<UIRenderable>())
                        {
                            renderable.internal_OnUIPointerEnter();
                        }
                    }

                    _lastPickedUITransform = pickedUITransform;
                }

                if (inputManager.WasPressedThisFrame(eMouseButton.Left))
                {
                    if (_lastPickedUITransform != null)
                    {
                        foreach (var renderable in _lastPickedUITransform.GetComponents<UIRenderable>())
                        {
                            renderable.internal_OnUIPointerDown();
                            _pointerDownRenderablesCache.Add(renderable);
                        }
                    }
                }

                if (inputManager.WasPressedThisFrame(eMouseButton.Right))
                {
                    if (_lastPickedUITransform != null)
                    {
                        foreach (var renderable in _lastPickedUITransform.GetComponents<UIRenderable>())
                        {
                            renderable.internal_OnUIRightClick();
                        }
                    }
                }

                if (inputManager.WasReleasedThisFrame(eMouseButton.Left))
                {
                    // alt+tab 등으로 focus 를 잃어버린 경우, pointer up 이벤트를 받지 못할 수 있음.
                    // 따라서, pointer down 된 모든 renderable 에 대해 pointer up 이벤트를 보내준다.
                    foreach (var renderable in _pointerDownRenderablesCache)
                    {
                        if (renderable != null)
                            renderable.internal_OnUIPointerUp();
                    }
                    _pointerDownRenderablesCache.Clear();
                }
            }
        }

        public override void internal_Invalidate()
        {
            UpdateProjection();
        }
    }
}
