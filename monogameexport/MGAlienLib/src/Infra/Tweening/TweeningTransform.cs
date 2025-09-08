using Microsoft.Xna.Framework;

namespace MGAlienLib.Tweening
{
    public abstract class TweeningTransform<T> : TweeningBase<T>
    {
        public Transform _target;
        protected abstract T Controller { get; set; }
        public TweeningTransform<T> Init(Transform target, T targetValue, float duration)
        {
            _target = target;
            base.Init(Controller, targetValue, duration);
            return this;
        }
    }

    public abstract class TweeingTransformVector3Type : TweeningTransform<Vector3>
    {
        protected override void OnUpdateValue(float r)
        {
            _currentValue = Vector3.Lerp(_initialValue, _targetValue, r);
            Controller = _currentValue;
        }
        protected override void OnIncrementalLoopReset()
        {
            _targetValue = _targetValue + (_targetValue - _initialValue);
            _initialValue = _currentValue;
        }
    }

    public abstract class TweeingTransformQuaternionType : TweeningTransform<Quaternion>
    {
        protected override void OnUpdateValue(float r)
        {
            _currentValue = Quaternion.Lerp(_initialValue, _targetValue, r);
            Controller = _currentValue;
        }
        protected override void OnIncrementalLoopReset()
        {
            _targetValue = _targetValue + (_targetValue - _initialValue);
            _initialValue = _currentValue;
        }
    }

    public class TweeingTransformPosition : TweeingTransformVector3Type
    {
        protected override Vector3 Controller
        {
            get => _target.position;
            set => _target.position = value;
        }
    }

    public class TweeingTransformScale : TweeingTransformVector3Type
    {
        protected override Vector3 Controller
        {
            get => _target.scale;
            set => _target.scale = value;
        }
    }

    public class TweeingTransformRotation : TweeingTransformQuaternionType
    {
        protected override Quaternion Controller
        {
            get => _target.rotation;
            set => _target.rotation = value;
        }
    }

    public class TweeingTransformLocalPosition : TweeingTransformVector3Type
    {
        protected override Vector3 Controller
        {
            get => _target.localPosition;
            set => _target.localPosition = value;
        }
    }

    public class TweeingTransformLocalScale : TweeingTransformVector3Type
    {
        protected override Vector3 Controller
        {
            get => _target.localScale;
            set => _target.localScale = value;
        }
    }

    public class TweeingTransformLocalRotation : TweeingTransformQuaternionType
    {
        protected override Quaternion Controller
        {
            get => _target.localRotation;
            set => _target.localRotation = value;
        }
    }

    public static class TransformExtension
    {
        public static void ActivateTweener<T>(this Transform _this, TweeningBase<T> tweener)
        {
            _this.StartMicroRoutine((dt, data) =>
            {
                tweener.Update(dt);
                return (tweener.IsPlaying(), data);
            }, tweener);
        }

        public static T DO<T, T2>(this Transform _this, T2 to, float duration) where T : TweeningTransform<T2>, new()
        {
            var tweener = new T().Init(_this, to, duration);
            ActivateTweener(_this, tweener);
            return tweener as T;
        }

        public static TweeingTransformPosition DOMove(this Transform _this, Vector3 to, float duration)
        {
            return DO< TweeingTransformPosition, Vector3>(_this, to, duration);
        }

        public static TweeingTransformScale DOScale(this Transform _this, Vector3 to, float duration)
        {
            return DO<TweeingTransformScale, Vector3>(_this, to, duration);
        }

        public static TweeingTransformRotation DORotate(this Transform _this, Quaternion to, float duration)
        {
            return DO<TweeingTransformRotation, Quaternion>(_this, to, duration);
        }

        public static TweeingTransformLocalPosition DOLocalMove(this Transform _this, Vector3 to, float duration)
        {
            return DO<TweeingTransformLocalPosition, Vector3>(_this, to, duration);
        }

        public static TweeingTransformLocalScale DOLocalScale(this Transform _this, Vector3 to, float duration)
        {
            return DO<TweeingTransformLocalScale, Vector3>(_this, to, duration);
        }

        public static TweeingTransformLocalRotation DOLocalRotate(this Transform _this, Quaternion to, float duration)
        {
            return DO<TweeingTransformLocalRotation, Quaternion>(_this, to, duration);
        }

    }
}
