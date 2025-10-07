using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class MeshRenderer : Renderable
    {
        private SharedMaterial.Reference _material;
        private SharedMesh.Reference _mesh;

        ~MeshRenderer()
        {
            Dispose(false);
        }

        public void Load(string assetAddress)
        {
            _mesh = SharedMesh.Get(assetAddress);
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
