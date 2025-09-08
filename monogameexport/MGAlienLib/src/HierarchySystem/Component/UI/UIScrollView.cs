using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MGAlienLib
{
    public class UIScrollView : UISpacer
    {
        public static readonly bool IsAddableFromInspector = true;

        private const float ScrollBarWidth = 16;
        private const float HandleHeight = 32;

        [SerializeField] protected AutoAtlasSpriteRenderer _scrollBarBG = null;
        [SerializeField] protected AutoAtlasSpriteRenderer _scrollBarHandle = null;
        [SerializeField] protected UISpacer _content = null;
        [SerializeField] protected float _scrollValue = 0;

        private bool _scrolling = false;
        private bool _inrect = false;

        public UISpacer content => _content;

        public override void Awake()
        {
            base.Awake();
            useAsUI = true;
            enableUIRaycast = true;
        }

        public override void Start()
        {
            base.Start();

            _scrollBarHandle.OnUIPointerDown += (e) =>
            {
                _scrolling = true;
            };
            _scrollBarHandle.OnUIPointerUp += (e) =>
            {
                _scrolling = false;
            };

            UITransform.SetDirty();
            content.UITransform.SetDirty();
            _scrollBarBG.UITransform.SetDirty();
            _scrollBarHandle.UITransform.SetDirty();
        }


        public override void Update()
        {
            base.Update();

            var mousePos = inputManager.GetMousePos();
            mousePos.Y = Screen.height - mousePos.Y; // y축 반전
            _inrect = UITransform.accumulatedRect.Contains(mousePos);

            // 전체 콘텐츠 높이와 뷰포트 높이 계산
            float contentHeight = content.UITransform.size.Y;
            float viewHeight = UITransform.size.Y;
            float contentMaxOffset = Mathf.Max(0, contentHeight - viewHeight);

            // 스크롤바 핸들의 높이와 이동 가능 범위 계산
            float handleHeight = _scrollBarHandle.UITransform.size.Y;
            float scrollArea = viewHeight - handleHeight - 32; // 핸들이 이동할 수 있는 실제 공간

            if (contentMaxOffset == 0)
            {
                _scrollValue = 0;
            }

            if (_scrolling)
            {
                var delta = inputManager.GetMousePosDelta();
                if (contentMaxOffset > 0)
                {
                    _scrollValue += delta.Y * 0.01f; // 콘텐츠 이동 거리에 비례
                    _scrollValue = Mathf.Clamp01(_scrollValue);
                }
            }

            // mouse wheel 로 scroll
            if (inputManager.GetMouseWheelDelta() != 0 && _inrect)
            {
                if (contentMaxOffset > 0)
                {
                    _scrollValue -= (inputManager.GetMouseWheelDelta() * 0.1f) / contentMaxOffset;
                    _scrollValue = Mathf.Clamp01(_scrollValue);
                }
            }

            // 핸들 위치 계산 (위에서 아래로)
            float handleY = Mathf.Lerp(0, scrollArea, _scrollValue);
            _scrollBarHandle.UITransform.position = new Vector2(0, -handleY);

            // 콘텐츠 위치 계산
            float contentY = Mathf.Lerp(0, -contentMaxOffset, _scrollValue);
            content.UITransform.position = new Vector2(0, -contentY);
            //content.UITransform.SetDirty();
        }

        public static UIScrollView Build(Transform parent,
            string name,
            RectangleF anchoredRect, float elevation,
            bool expandWidth = false, bool expandHeight = false, string layer = "UI")
        {
            var scrollView = UISpacer.Build<UIScrollView>(parent, 
                anchoredRect, 0.1f,
                expandWidth, expandHeight, 
                layer: layer);
            scrollView.useAsUI = true;
            scrollView.UITransform.expandWidthToParent = expandWidth;
            scrollView.UITransform.expandHeightToParent = expandHeight;

            scrollView._scrollBarBG = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(scrollView.transform,
                $"{name}_scrollBarBG", "art/UI/white.png", true, false,
                new RectangleF(0, 0, ScrollBarWidth, ScrollBarWidth), 0.5f,
                color: (new Color(1f,1f,1f,.5f)).Dimming(.5f),
                pivot: Vector2.One, anchor: Vector2.One,
                layer: layer
                );
            scrollView._scrollBarBG.UITransform.expandHeightToParent = true;

            scrollView._scrollBarHandle = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(scrollView.transform,
                $"{name}_scrollBarHandle", "art/UI/white.png", true, false,
                new RectangleF(0, 0, ScrollBarWidth, HandleHeight), .6f,
                color: Color.White,
                pivot: Vector2.One, anchor: Vector2.One,
                layer: layer
                );
            scrollView._scrollBarHandle.enableUIRaycast = true;

            scrollView._content = UISpacer.Build<UISpacer>(scrollView.transform,
                new RectangleF(0, 0, anchoredRect.Width, anchoredRect.Height), 0.1f,
                layer: layer
                );
            
            var mask = scrollView.AddComponent<UIMasking>();
            mask.useAsUI = true;

            return scrollView;
        }
    }
}
