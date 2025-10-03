
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 텍스처를 관리하는 클래스를 나타냅니다.
    /// </summary>
    public class SharedTexture: SharedAsset<Texture2D>
    {
        /// <summary>
        /// 지정된 소스와 주소로부터 공유 텍스처를 가져옵니다.
        /// </summary>
        /// <param name="address">자원의 주소</param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter</param>
        private SharedTexture(string address, object parameters) : base(address, parameters)
        {
        }

        /// <summary>
        /// 지정된 주소로부터 공유 텍스처를 가져옵니다.
        /// </summary>
        /// <param name="address">자원의 주소</param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter</param>
        /// <returns></returns>
        public static Reference Get(string address, object parameters)
        {
            return manager.Get(address, parameters, (a, p) => new SharedTexture(a, p));
        }

        /// <summary>
        /// 지정된 소스와 주소로부터 공유 텍스처를 가져옵니다.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="width">override 할 가로 크기. 이 크기대로 줄여서 load 한다</param>
        /// <param name="height">override 할 세로 크기. 이 크기대로 줄여서 load 한다</param>
        /// <returns></returns>
        public static Reference Get(string address, int width = 0, int height = 0)
        {
            return manager.Get(address, new object[] { width, height}, (a, p) =>  new SharedTexture(a, p));
        }

        /// <summary>
        /// 공유중인 텍스쳐를 찾을 수 없을 때. 새로운 텍스처를 생성합니다.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter</param>
        /// <returns></returns>
        protected override Texture2D CreateAsset(string address, object parameters)
        {
            if (parameters != null)
            {
                var array = parameters as object[];
                var width = (int)array[0];
                var height = (int)array[1];
                return assetManager.GetTexture2D(address, width, height);
            }
            else
                return assetManager.GetTexture2D(address);

        }
    }
}
