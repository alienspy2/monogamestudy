
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 공유 자원을 참조 카운팅 방식으로 관리하는 추상 클래스입니다.
    /// </summary>
    public abstract class AssetBase : MGDisposableObject
    {

        /// <summary>
        /// 자원의 고유 주소입니다.
        /// 작업 중에는 raw file 로 사용하다가, 패킹된 파일로 전환할 때 변경사항을 최소화하기 위해.
        /// 자원의 source 종류가 바뀌어도 주소는 유지됩니다.
        /// 따라서, 다른 source 에 같은 주소를 가진 자원은 같은 자원이어야 합니다.
        /// 기본적으로 파일 경로를 사용합니다.
        /// </summary>
        public string address { get; protected set; }

        protected AssetManager assetManager => GameBase.Instance.assetManager;
        protected ShaderManager shaderManager => GameBase.Instance.shaderManager;
    }
}
