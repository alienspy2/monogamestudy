
namespace MGAlienLib
{
    /// <summary>
    /// 레이어 관리 클래스
    /// </summary>
    public static class LayerMask
    {
        /// <summary>
        /// 레이어 이름을 비트마스크로 변환합니다.
        /// </summary>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static int GetMask(params string[] layerNames)
        {
            int mask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                var layer = GameBase.Instance.layerManager.GetLayerInfo(layerNames[i]);
                mask |= layer.bitMask;
            }
            return mask;
        }

        /// <summary>
        /// 레이어 이름을 레이어 인덱스로 변환합니다.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static int NameToLayer(string layerName)
        {
            var layer = GameBase.Instance.layerManager.GetLayerInfo(layerName);
            return layer.index;
        }

        /// <summary>
        /// 레이어 인덱스를 레이어 이름으로 변환합니다.
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static string LayerToName(int layerIndex)
        {
            var layer = GameBase.Instance.layerManager.GetLayerInfo(layerIndex);
            return layer.name;
        }
    }

    /// <summary>
    /// 레이어 관리 클래스
    /// </summary>
    public class LayerManager : ManagerBase
    {
        public const int MaxLayerCount = 31;

        public class Layer
        {
            public int bitMask => (1 << index);
            public string name;
            public int index;
        }

        private Layer[] layers = null;

        public LayerManager(GameBase owner) : base(owner)
        {
            layers = new Layer[MaxLayerCount];
            for (int i = 0; i < 31; i++)
            {
                layers[i] = new Layer
                {
                    name = $"",
                    index = i
                };
            }

            layers[0].name = "Default";
            layers[1].name = "UI";
        }

        /// <summary>
        /// 레이어 정보를 가져옵니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Layer GetLayerInfo(int index)
        {
            return layers[index];
        }

        /// <summary>
        /// 레이어 정보를 가져옵니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Layer GetLayerInfo(string name)
        {
            for (int i = 0; i < MaxLayerCount; i++)
            {
                if (layers[i].name == name)
                {
                    return layers[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 레이어 정보를 설정합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        public void SetLayerInfo(int index, string name)
        {
            layers[index].name = name;
        }
    }
}
