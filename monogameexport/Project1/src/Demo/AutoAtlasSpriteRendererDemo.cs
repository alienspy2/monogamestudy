using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace MGAlienLib
{
    /// <summary>
    /// AutoAtlasSpriteRenderer 를 테스트하는 데모입니다.
    /// 임의의 sprite 를 Load 하면,
    /// 실시간으로 Texture 를 atlas manager 에 추가하고,
    /// batching 을 통해 그리게 됩니다.
    /// 미리 atlas 를 만들어 두기 애매한 경우 사용 합니다.
    /// dds 와 같은 texture 압축을 사용할 수 없기 때문에,
    /// static atlas 가 더 효츌적입니다.
    /// 실시간으로 texture 를 생성하는 procedural texture 와 같이 사용하면
    /// 유용합니다
    /// </summary>
    public class AutoAtlasSpriteRendererDemo : ComponentBase
    {
        private List<AutoAtlasSpriteRenderer> testAASpriteRenderers;
        public float spacing = 40;

        public override void Awake()
        {
            base.Awake();

            var filenames = assetManager.SearchFiles("ui Lorc icon")[0..400];
            testAASpriteRenderers = new();

            int i_file = 0;
            foreach (var filename in filenames)
            {
                int x = i_file % ((int)Mathf.Sqrt(filenames.Count));
                int y = i_file / ((int)Mathf.Sqrt(filenames.Count));
                var size = Vector2.One * spacing;
                var spriteObj = hierarchyManager.CreateGameObject($"AA sprite {i_file}", transform);
                spriteObj.transform.position = new Vector3(x * size.X * 1.1f, y * size.Y * 1.1f, 0);

                var aa = spriteObj.AddComponent<AutoAtlasSpriteRenderer>();
                aa.Load(filename, true, 100, 100);
                aa.color = Color.White;
                aa.size = size;

                testAASpriteRenderers.Add(aa);

                i_file++;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach(var aa in testAASpriteRenderers)
            {
                Destroy(aa.gameObject);
            }
        }
    }
}
