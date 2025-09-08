
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 렌더링 가능한 컴포넌트의 기본 클래스입니다.
    /// </summary>
    public abstract class Renderable : ComponentBase
    {
        /// <summary>
        /// 렌더링을 수행합니다.
        /// 최적화를 위해, batching 을 하는 경우가 있으므로,
        /// 구현에 따라 이 안에서 실제로 draw call 이 발생 하지 않을 수도 있습니다.
        /// render target 에 그리는 경우, 이 안에서 draw call 이 발생합니다.
        /// </summary>
        /// <param name="renderState"></param>
        public virtual void Render(RenderState renderState)
        {
        }

        /// <summary>
        /// 렌더링을 수행합니다.
        /// 내부적으로만 사용됩니다.
        /// </summary>
        /// <param name="renderState"></param>
        public virtual void internal_Render(RenderState renderState)
        {
            Render(renderState);
        }

    }
}
