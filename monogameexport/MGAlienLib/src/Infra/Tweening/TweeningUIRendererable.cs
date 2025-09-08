
using Microsoft.Xna.Framework;

namespace MGAlienLib.Tweening
{
    public abstract class TweeningUIRendererable<T> : TweeningBase<T>
    {
        public UIRenderable _target;
        protected abstract T Controller { get; set; }
        public TweeningUIRendererable<T> Init(UIRenderable target, T targetValue, float duration)
        {
            _target = target;
            base.Init(Controller, targetValue, duration);
            return this;
        }
    }

    public class TweeningUIRenderableColor : TweeningUIRendererable<Color>
    {
        protected override Color Controller
        {
            get => _target.color;
            set => _target.color = value;
        }
        protected override void OnUpdateValue(float r)
        {
            _currentValue = Color.Lerp(_initialValue, _targetValue, r);
            Controller = _currentValue;
        }

        protected override void OnIncrementalLoopReset()
        {
            Vector4 colorV4 = _targetValue.ToVector4() + _targetValue.ToVector4() - _initialValue.ToVector4();
            _targetValue = new Color(colorV4);
            _initialValue = _currentValue;
        }
    }

    public class TweeningUIRenderableSize : TweeningUIRendererable<Vector2>
    {
        protected override Vector2 Controller
        {
            get => _target.size;
            set => _target.size = value;
        }

        protected override void OnUpdateValue(float r)
        {
            _currentValue = Vector2.Lerp(_initialValue, _targetValue, r);
            Controller = _currentValue;
        }

        protected override void OnIncrementalLoopReset()
        {
            _targetValue = _targetValue + (_targetValue - _initialValue);
            _initialValue = _currentValue;
        }
    }

    public static class AutoAtlasSpriteRenderExtention
    {        
        public static void ActivateTweener<T>(this UIRenderable _this, TweeningBase<T> tweener)
        {
            _this.StartMicroRoutine((dt, data) =>
            {
                tweener.Update(dt);
                return (tweener.IsPlaying(), data);
            }, tweener);
        }


        public static T1 DO<T1, T2>(this UIRenderable _this, T2 color, float duration) where T1 : TweeningUIRendererable<T2>, new()
        {
            var tweener = new T1().Init(_this, color, duration);
            ActivateTweener(_this, tweener);
            return tweener as T1;
        }

        public static TweeningUIRenderableColor DOColor(this UIRenderable _this, Color color, float duration)
        {
            return DO<TweeningUIRenderableColor, Color>(_this, color, duration);
        }

        public static TweeningUIRenderableSize DOSize(this UIRenderable _this, Vector2 size, float duration)
        {
            return DO<TweeningUIRenderableSize, Vector2>(_this, size, duration);
        }
    }



}
