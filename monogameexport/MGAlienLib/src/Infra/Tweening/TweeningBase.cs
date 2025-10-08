
using MGAlienLib.Utility;
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib.Tweening
{
    public enum eLoopType
    {
        None,
        Restart,
        Yoyo,
        Incremental
    }

    public class TweeningBase<T>
    {
        public eEasingType _easingType;
        public T _initialValue;
        public T _currentValue;
        public T _targetValue;
        public float _duration;
        public float _elapsedTime;
        public eLoopType _loopType;
        public int _loopCount;
        public int _loopCountMax;
        public Action _onComplete;

        public TweeningBase<T> Init(T initialValue, T targetValue, float duration)
        {
            _easingType = eEasingType.Linear;
            _initialValue = initialValue;
            _currentValue = initialValue;
            _targetValue = targetValue;
            _duration = duration;
            _elapsedTime = 0;
            _loopType = eLoopType.None;
            _loopCount = 1;
            _loopCountMax = 1;
            _onComplete = null;
            return this;
        }

        public bool IsComplete()
        {
            if (_loopCount < _loopCountMax)
            {
                return false;
            }

            return _elapsedTime >= _duration;
        }

        public TweeningBase<T> OnComplete(Action onComplete)
        {
            _onComplete = onComplete;
            return this;
        }

        public bool IsPlaying() => !IsComplete();

        public TweeningBase<T> SetEase(eEasingType easingType)
        {
            _easingType = easingType;
            return this;
        }

        public TweeningBase<T> From(T _from)
        {
            _initialValue = _from;
            _currentValue = _from;
            return this;
        }

        public void SetLoops(int count, eLoopType loopType)
        {
            _loopCount = 1;
            _loopCountMax = count;
            _loopType = loopType;
        }

        public void Update(float deltaTime)
        {
            _elapsedTime += deltaTime;
            if (_elapsedTime > _duration)
            {
                if (_loopCountMax == -1 || _loopCount < _loopCountMax)
                {
                    _loopCount++;
                    _elapsedTime = 0;
                    if (_loopType == eLoopType.Yoyo)
                    {
                        var temp = _initialValue;
                        _initialValue = _targetValue;
                        _targetValue = temp;
                    }
                    else if (_loopType == eLoopType.Restart)
                    {
                        _currentValue = _initialValue;
                    }
                    else if (_loopType == eLoopType.Incremental)
                    {
                        OnIncrementalLoopReset();
                    }
                }
                else
                {
                    _currentValue = _targetValue;
                }
            }

            var r = TweenerUtility.TweenFloat(_easingType, 0, 1, _duration, _elapsedTime);
            OnUpdateValue(r);

            if (IsComplete())
            {
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }


        protected virtual void OnUpdateValue(float r) { }

        protected virtual void OnIncrementalLoopReset() { }
    }

}
