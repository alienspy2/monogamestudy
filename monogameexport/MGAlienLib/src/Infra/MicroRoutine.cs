
namespace MGAlienLib
{
    /// <summary>
    /// 매 프레임마다 호출되는 델리게이트를 갖는 미니 루틴
    /// 소유자가 무효화 되면 루틴이 호출되지 않음
    /// coroutine 과 비슷하지만, 대기가 불가능하고, 조금 더 가볍다
    /// routine 의 반환값이 false 면 루틴이 종료됨
    /// </summary>
    public class MicroRoutine
    {
        public delegate (bool, object) MicroRoutineDelegate(float deltaTime, object userdata);
        private MicroRoutineDelegate _routine;
        private object _userdata;

        public MicroRoutine(MicroRoutineDelegate handle, object userdata)
        {
            _routine = handle;
            _userdata = userdata;
        }

        public bool Update(float deltaTime)
        {
            if (_routine == null)
                return false;

            (bool moveNext, object updatedData) = _routine(deltaTime, _userdata);
            _userdata = updatedData;

            return moveNext;
        }
    }
}
