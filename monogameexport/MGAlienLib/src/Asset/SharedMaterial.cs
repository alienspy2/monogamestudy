

using Microsoft.Xna.Framework.Graphics;

namespace MGAlienLib
{
    public class SharedMaterial : SharedAsset<Material>
    {
        private SharedMaterial(string address, object parameters) : base(address, parameters)
        {
        }

        public SharedMaterial(string address, Material externalAsset) : base(address, null)
        {
            internal_SetExternalAsset(externalAsset);
        }

        public static Reference Get(int renderPriority, Shader shader, SharedTexture.Reference tex)
        {
            var address = $"{renderPriority}:{shader.name}:{tex.address}";
            var parameters = new object[] { renderPriority, shader, tex.asset };

            var newMat = manager.Get(address, parameters, (a, p) => new SharedMaterial(a, p));

            return newMat;
        }

        public static Reference Get(string address)
        {
            var newMat = manager.Get(address, null, (a, p) => new SharedMaterial(a, p));

            return newMat;
        }


        protected override Material CreateAsset(string address, object parameters)
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
