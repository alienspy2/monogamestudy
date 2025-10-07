using Microsoft.Xna.Framework.Graphics;

namespace MGAlienLib
{
    public class MeshRenderer : Renderable
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField(browseFile: true)] protected string _assetAddress;

        private SharedMaterial.Reference _material;
        private SharedMesh.Reference _mesh;

        ~MeshRenderer()
        {
            Dispose(false);
        }

        public void Load(string assetAddress)
        {
            _mesh = SharedMesh.Get(assetAddress);
            _assetAddress = assetAddress;
        }

        public void LoadMaterial(string shaderName)
        {
            var shader = shaderManager.GetShaderByName(shaderName);
            _material = SharedMaterial.Get(0, shader, game.defaultAssets.whiteTexture);
            _material.asset.cullMode = CullMode.CullClockwiseFace;
        }

        public override void Render(RenderState renderState)
        {
            if (_material == null) return;
            if (_mesh == null) return;

            renderState.CheckAndAddCommand(() =>
            {
                if (_material == null) return;
                if (_material.asset == null) return;
                if (_material.asset.shader == null) return;

                if (_mesh == null) return; 
                if (_mesh.asset == null) return;

                var material = _material.asset;
                var cam = renderState.camera;

                material.ApplyParams();
                material.shader.effect.Parameters["ViewProjection"].SetValue(cam.matViewProjection);
                material.shader.effect.Parameters["World"]?.SetValue(transform.localToWorldMatrix);

                foreach (EffectPass pass in material.shader.effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                        _mesh.asset.v, 0, _mesh.asset.v.Length,
                        _mesh.asset.indices, 0, _mesh.asset.indices.Length / 3);
                }
            });

        }


        public override void internal_Invalidate()
        {
            base.internal_Invalidate();
            if (!_assetAddress.IsNullOrEmpty())
            {
                Load(_assetAddress);
            }
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            if (!_assetAddress.IsNullOrEmpty())
            {
                Load(_assetAddress);
            }
        }

        public override void OnDispose()
        {
            base.OnDispose();

            // 관리되는 리소스 해제
            if (_mesh != null)
            {
                _mesh.Release();
                _mesh = null;
            }

            if (_material != null)
            {
                _material.Release();
                _material = null;
            }
        }

    }
}
