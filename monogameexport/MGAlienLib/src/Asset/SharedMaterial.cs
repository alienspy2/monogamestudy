

using Microsoft.Xna.Framework.Graphics;

namespace MGAlienLib
{
    public class SharedMaterial : SharedAsset<Material>
    {
        private SharedMaterial(eAssetSource source, string address, object parameters) : base(source, address, parameters)
        {
        }

        public SharedMaterial(string address, Material externalAsset) : base(eAssetSource.Dummy, address, null)
        {
            internal_SetExternalAsset(externalAsset);
        }

        public static Reference Get(int renderPriority, Shader shader, SharedTexture.Reference tex)
        {
            var address = $"{renderPriority}:{shader.name}:{tex.address}";
            var parameters = new object[] { renderPriority, shader, tex.asset };

            var newMat = manager.Get(eAssetSource.Dummy, address, parameters, (s, a, p) => new SharedMaterial(s, a, p));

            return newMat;
        }

        public static Reference Get(string address)
        {
            var newMat = manager.Get(eAssetSource.Dummy, address, null, (s, a, p) => new SharedMaterial(s, a, p));

            return newMat;
        }


        protected override Material CreateAsset(eAssetSource source, string address, object parameters)
        {
            var newMat = new Material();

            if (parameters != null)
            {
                var array = parameters as object[];
                int renderPriority = (int)array[0];
                var shader = (Shader)array[1];
                var tex = (Texture2D)array[2];

                newMat.renderPriority = renderPriority;
                newMat.shader = shader;
                newMat.SetTexture("_MainTex", tex);

                return newMat;
            }

            return newMat;
        }
    }
}
