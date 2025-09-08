
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib
{
    public class UIRenderable : Renderable
    {
        [SerializeField] protected bool _useAsUI = false;
        [SerializeField] protected bool _enableUIRaycast = false;

        // UI 로 사용할 때에는 1,1, 0,0 으로 고정. 대신 UITransform 의 것을 사용
        // 여기 있는 _size 와 _pivot 은 UI 로 사용하지 않을 때만 사용
        [SerializeField] protected Vector2 _size = Vector2.One; // 크기를 저장하는 필드 (width, height)
        [SerializeField] protected Vector2 _pivot = Vector2.One * .5f;
        [SerializeField] protected Color _color = Color.White;

        private bool _uiPointerHolding = false;

        protected Action<UIRenderable> _OnUIPointerEnter = null;
        protected Action<UIRenderable> _OnUIPointerExit = null;
        protected Action<UIRenderable> _OnUIPointerDown = null;
        protected Action<UIRenderable> _OnUIPointerUp = null;
        protected Action<UIRenderable> _OnUICommand = null;
        protected Action<UIRenderable> _OnUIRightClick = null;

        public bool useAsUI 
        {
            get => _useAsUI;
            set
            {
                _useAsUI = value;
                if (_useAsUI)
                {
                    size = Vector2.One;
                    pivot = Vector2.Zero;

                    var _uiTransformCache = GetComponent<UITransform>();

                    if (_uiTransformCache == null)
                    {
                        AddComponent<UITransform>();
                    }
                }
                else
                {
                    var _uiTransformCache = GetComponent<UITransform>();

                    if (_uiTransformCache != null)
                    {
                        RemoveComponent(_uiTransformCache);
                    }
                }
            }
        }

        public Vector2 size 
        {
            get => _size;
            set
            {
                if (_useAsUI)
                {
                    _size = Vector2.One;
                }
                else
                {
                    _size = value;
                }
            }
        }

        public float width
        {
            get => _size.X;
            set
            {
                _size.X = value;
            }
        }

        public float height
        {
            get => _size.Y;
            set
            {
                _size.Y = value;
            }
        }

        public Vector2 pivot 
        {
            get => _pivot; 
            set
            {
                if (_useAsUI)
                {
                    _pivot = Vector2.Zero;
                }
                else
                {
                    _pivot = value;
                }
            }
        }

        public virtual Color color 
        { 
            get => _color;
            set => _color = value;
        }

        public UITransform UITransform
        {
            get
            {
                var _uiTransformCache = GetComponent<UITransform>();
                return _uiTransformCache;
            }
        }

        /// <summary>
        /// 마우스가 UITransform 의 영역 안으로 들어올 때 호출되는 이벤트입니다.
        /// UI 로 사용할 때에만 사용가능합니다.
        /// _enableRaycast 가 켜져야 합니다.
        /// 같은 gameObject 에 UITransform 도 같이 있어야 합니다
        /// </summary>
        public Action<UIRenderable> OnUIPointerEnter
        {
            get => _OnUIPointerEnter;
            set => _OnUIPointerEnter = value;
        }

        /// <summary>
        /// 마우스가 UITransform 의 영역 밖으로 나갈 때 호출되는 이벤트입니다.
        /// UI 로 사용할 때에만 사용가능합니다.
        /// _enableRaycast 가 켜져야 합니다.
        /// 같은 gameObject 에 UITransform 도 같이 있어야 합니다
        /// </summary>
        public Action<UIRenderable> OnUIPointerExit
        {
            get => _OnUIPointerExit;
            set => _OnUIPointerExit = value;
        }

        /// <summary>
        /// 마우스가 UITransform 의 영역 안에서 눌렸을 때 호출되는 이벤트입니다.
        /// UI 로 사용할 때에만 사용가능합니다.
        /// _enableRaycast 가 켜져야 합니다.
        /// 같은 gameObject 에 UITransform 도 같이 있어야 합니다
        /// </summary>
        public Action<UIRenderable> OnUIPointerDown
        {
            get => _OnUIPointerDown;
            set => _OnUIPointerDown = value;
        }

        /// <summary>
        /// 마우스가 UITransform 의 영역 안에서 뗴어질 때 호출되는 이벤트입니다.
        /// </summary>
        public Action<UIRenderable> OnUIPointerUp
        {
            get => _OnUIPointerUp;
            set => _OnUIPointerUp = value;
        }

        /// <summary>
        /// 이 요소의 영역 안에서 마우스가 눌렸다가, 영역 안에서 뗴어질 때 호출됩니다.
        /// 둘 중 하나라도 해당하지 않으면 호출되지 않습니다.
        /// 
        /// UI 로 사용할 때에만 사용가능합니다.
        /// _enableRaycast 가 켜져야 합니다.
        /// 같은 gameObject 에 UITransform 도 같이 있어야 합니다
        /// </summary>
        public Action<UIRenderable> OnUICommand
        {
            get => _OnUICommand;
            set => _OnUICommand = value;
        }

        public Action<UIRenderable> OnUIRightClick
        {
            get => _OnUIRightClick;
            set => _OnUIRightClick = value;
        }

        /// <summary>
        /// UI 레이캐스트를 활성화할지 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool enableUIRaycast
        {
            get => _enableUIRaycast;
            set
            {
                if (useAsUI == false)
                    throw new Exception("This object is not UI.");
                _enableUIRaycast = value;
            }
        }


        /// <summary>
        /// 렌더링을 수행합니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        /// <param name="renderState"></param>
        public override void internal_Render(RenderState renderState)
        {
            base.internal_Render(renderState);

            if (_enableUIRaycast && renderState.camera.useAsUI)
            {
                UITransform.internal_OnRender(renderState);
            }
        }

        /// <summary>
        /// UI 요소에 마우스가 들어왔을 때 호출됩니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        public virtual void internal_OnUIPointerEnter()
        {
            if (_enableUIRaycast)
            {
                OnUIPointerEnter?.Invoke(this);
            }
        }


        /// <summary>
        /// UI 요소에 마우스가 나갈 때 호출됩니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        public virtual void internal_OnUIPointerExit()
        {
            if (_enableUIRaycast)
            {
                _uiPointerHolding = false;
                OnUIPointerExit?.Invoke(this);
            }
        }


        /// <summary>
        /// UI 요소에 마우스 버튼이 눌렸을 때 호출됩니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        public virtual void internal_OnUIPointerDown()
        {
            if (_enableUIRaycast)
            {
                _uiPointerHolding = true;
                OnUIPointerDown?.Invoke(this);
            }
        }

        public virtual void internal_OnUIRightClick()
        {
            if (_enableUIRaycast)
            {
                OnUIRightClick?.Invoke(this);
            }
        }

        /// <summary>
        /// UI 요소에 마우스 버튼이 떼어졌을 때 호출됩니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        public virtual void internal_OnUIPointerUp()
        {
            if (_enableUIRaycast)
            {
                if (_uiPointerHolding)
                {
                    _uiPointerHolding = false;
                    OnUICommand?.Invoke(this);
                }

                OnUIPointerUp?.Invoke(this);
            }
        }
    }
}
