
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// UI 요소들을 수직으로 쌓아주는 컴포넌트입니다.
    /// stacker 를 사용하면 자식들의 위치가 자동으로 조정해줍니다.
    /// 요소의 가로 크기는 stacker의 가로 크기에 맞춰집니다.
    /// 요소의 세로 크기는 자동으로 조정되지 않습니다. (나중에 기능 추가될 가능성 있음)
    /// </summary>
    public class UIVStacker : ComponentBase
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField] protected float _topMargin = 5f;
        [SerializeField] protected float _bottomMargin = 5f;
        [SerializeField] protected float _leftMargin = 5f;
        [SerializeField] protected float _rightMargin = 5f;
        [SerializeField] protected float _spacing = 0f;
        [SerializeField] protected bool _autoSize = true;
        [SerializeField] protected bool _expandChildWidth = false;

        public float topMargin { get => _topMargin; set => _topMargin = value; }
        public float bottomMargin { get => _bottomMargin; set => _bottomMargin = value; }
        public float leftMargin { get => _leftMargin; set => _leftMargin = value; }
        public float rightMargin { get => _rightMargin; set => _rightMargin = value; }
        public float spacing { get => _spacing; set => _spacing = value; }
        public bool autoSize { get => _autoSize; set => _autoSize = value; }

        public bool expandChildWidth { get => _expandChildWidth; set => _expandChildWidth = value; }

        public override void LateUpdate()
        {
            var children = transform.GetChildren();
            var uit = GetComponent<UITransform>();
            float y = topMargin;

            // stacker 를 쓰면 anchor와 pivot을 모두 left top으로 설정으로 강제한다.
            uit.pivot = uit.anchor = Vector2.UnitY;

            float maxWidth = 0;
            foreach (var child in children)
            {
                if (child == null) return;
                if (child.gameObject.active == false) continue;

                var childTransform = child.GetComponent<UITransform>();
                if (childTransform == null) continue;

                // stacker 를 쓰면 anchor와 pivot을 모두 left top으로 설정으로 강제한다.
                childTransform.pivot = childTransform.anchor = Vector2.UnitY;

                var anchoredRect = childTransform.anchoredRect;
                anchoredRect.X = leftMargin;
                anchoredRect.Y = -y;
                if (expandChildWidth)
                {
                    anchoredRect.Width = uit.anchoredRect.Width - leftMargin * 2;
                }
                maxWidth = Mathf.Max(maxWidth, anchoredRect.Width);
                childTransform.anchoredRect = anchoredRect;

                // 다음 Y 위치 계산
                y += (anchoredRect.Height + spacing);
            }

            // 마지막 요소의 spacing 을 빼준다.
            y -= spacing;

            if (autoSize)
            {
                var rect = transform.GetComponent<UITransform>().anchoredRect;
                rect.Width = leftMargin + maxWidth + rightMargin;
                rect.Height = y + bottomMargin;
                transform.GetComponent<UITransform>().anchoredRect = rect;
            }
        }

        public void SetMargins(float leftMargin, float topMargin, float rightMargin, float bottomMargin, float spacing)
        {
            this.leftMargin = leftMargin;
            this.topMargin = topMargin;
            this.rightMargin = rightMargin;
            this.bottomMargin = bottomMargin;
            this.spacing = spacing;
        }
    }
}
