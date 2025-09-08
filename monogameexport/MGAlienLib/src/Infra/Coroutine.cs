

using System;
using System.Collections;

namespace MGAlienLib
{
    public class Coroutine
    {
        private IEnumerator _routine;
        private object _currentYield;

        public Coroutine(IEnumerator handle)
        {
            _routine = handle;
            _currentYield = null;
        }

        public bool Update(float deltaTime)
        {
            if (_currentYield is WaitBase wait)
            {
                if (!wait.IsDone(deltaTime))
                    return true; // 아직 대기 중
                wait.Reset(); // 다음 사용을 위해 리셋
            }

            if (_routine.MoveNext())
            {
                _currentYield = _routine.Current;
                return true; // 계속 실행 중
            }

            return false; // 코루틴 완료
        }
    }

    public abstract class WaitBase
    {
        public abstract bool IsDone(float deltaTime);
        public abstract void Reset();
    }

    public class WaitForSeconds : WaitBase
    {
        private float _waitTime;
        private float _elapsedTime;

        public WaitForSeconds(float seconds)
        {
            _waitTime = seconds;
            _elapsedTime = 0f;
        }

        public override bool IsDone(float deltaTime)
        {
            _elapsedTime += deltaTime;
            return _elapsedTime >= _waitTime;
        }

        public override void Reset()
        {
            _elapsedTime = 0f;
        }
    }

    public class WaitUntil : WaitBase
    {
        private Func<bool> _condition;
        public WaitUntil(Func<bool> condition) => _condition = condition;
        public override bool IsDone(float deltaTime) => _condition();

        public override void Reset() { }
    }
}
