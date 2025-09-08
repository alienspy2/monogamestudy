using MGAlienLib;
using MGAlienLib.Utility;
using MGAlienLib.Tweening;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class TweenerDemo : ComponentBase
    {
        public float duration = 3;
        private List<SpriteRenderer> balls;

        public override void Awake()
        {
            base.Awake();

            balls = new ();

            eEasingType[] easingTypes = new eEasingType[]
            {
                eEasingType.Linear,
                eEasingType.EaseInQuad,
                eEasingType.EaseOutQuad,
                eEasingType.EaseInOutQuad,
                eEasingType.EaseInCubic,
                eEasingType.EaseOutCubic,
                eEasingType.EaseInOutCubic,
                eEasingType.EaseInQuart,
                eEasingType.EaseOutQuart,
                eEasingType.EaseInOutQuart,
                eEasingType.EaseInQuint,
                eEasingType.EaseOutQuint,
                eEasingType.EaseInOutQuint,
                eEasingType.EaseInSine,
                eEasingType.EaseOutSine,
                eEasingType.EaseInOutSine,
                eEasingType.EaseInExpo,
                eEasingType.EaseOutExpo,
                eEasingType.EaseInOutExpo,
                eEasingType.EaseInCirc,
                eEasingType.EaseOutCirc,
                eEasingType.EaseInOutCirc,
                eEasingType.EaseInBack,
                eEasingType.EaseOutBack,
                eEasingType.EaseInOutBack,
                eEasingType.EaseInElastic,
                eEasingType.EaseOutElastic,
                eEasingType.EaseInOutElastic,
                eEasingType.EaseInBounce,
                eEasingType.EaseOutBounce,
                eEasingType.EaseInOutBounce
            };

            int spacing = 100;
            int hightGap = 150;

            for (int i = 0; i < easingTypes.Length; i++)
            {
                float x = (i % 8) * spacing;
                float y = (i / 8) * hightGap;
                var aa = hierarchyManager.CreateGameObject($"aa {i}", transform).AddComponent<SpriteRenderer>();
                aa.Load("art/UI/ball.png");
                aa.transform.scale = Vector3.One * .5f;
                aa.transform.DOMove(new Vector3(x, y + 100, 0), duration).
                    From(new Vector3(x, y, 0)).
                    SetEase(easingTypes[i]).
                    SetLoops(-1, eLoopType.Yoyo);

                var textObj = hierarchyManager.CreateGameObject($"text {i}", transform);
                var text = textObj.AddComponent<TextRenderer>();
                text.text = easingTypes[i].ToString();
                text.fontSize = 16;
                text.transform.scale = Vector3.One * 0.5f;
                text.HAlign = eHAlign.Center;
                text.VAlign = eVAlign.Middle;
                text.color = Color.White;
                text.SetOutline(true);
                text.transform.position = new Vector3(x, y-30, 0);

                balls.Add(aa);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var aa in balls)
            {
                Destroy(aa.gameObject);
            }
        }
    }
}
