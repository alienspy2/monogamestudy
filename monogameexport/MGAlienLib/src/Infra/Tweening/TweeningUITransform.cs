
using Microsoft.Xna.Framework;

namespace MGAlienLib.Tweening
{
    public abstract class TweeningUITransform<T> : TweeningBase<T>
    {
        public UITransform _target;
        protected abstract T Controller { get; set; }

        public TweeningUITransform<T> Init(UITransform target, T targetValue, float duration)
        {
            _target = target;
            base.Init(Controller, targetValue, duration);
            return this;
        }
    }

    public abstract class TweeningUITransformVector2Type : TweeningUITransform<Vector2>
    {
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

    public class TweeningUITransformMove : TweeningUITransformVector2Type
    {
        protected override Vector2 Controller
        {
            get => _target.position;
            set => _target.position = value;
        }
    }

    public class TweeningUITransformSize : TweeningUITransformVector2Type
    {
        protected override Vector2 Controller
        {
            get => _target.size;
            set => _target.size = value;
        }
    }

    public static class UITransformExtension
    {
        public static void ActivateTweener<T>(this UITransform _this, TweeningBase<T> tweener)
        {
            _this.StartMicroRoutine((dt, data) =>
            {
                tweener.Update(dt);
                return (tweener.IsPlaying(), data);
            }, tweener);
        }

        public static T DO<T, T2>(this UITransform _this, T2 to, float duration) where T : TweeningUITransform<T2>, new()
        {
            var tweener = new T().Init(_this, to, duration);
            ActivateTweener(_this, tweener);
            return tweener as T;
        }

        public static TweeningUITransformMove DOMove(this UITransform _this, Vector2 to, float duration)
        {
            return DO<TweeningUITransformMove, Vector2>(_this, to, duration);
        }

        public static TweeningUITransformSize DOSize(this UITransform _this, Vector2 to, float duration)
        {
            return DO<TweeningUITransformSize, Vector2>(_this, to, duration);
        }

    }
}
