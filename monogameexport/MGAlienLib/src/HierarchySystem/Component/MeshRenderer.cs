using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGAlienLib
{
    public class MeshRenderer : Renderable
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField(browseFile: true)] protected string _assetAddress;

        private SharedMaterial.Reference _material;
        private SharedMesh.Reference _mesh;
        private SharedAsset<Material> _unmanagedMaterial;

        public SharedMaterial.Reference material
        {
            get => _material;
        }

        ~MeshRenderer()
        {
            Dispose(false);
        }

        /// <summary>
        /// sharing 된 matrial 의 값을 바꾸면,
        /// 같은 material 을 사용한 모든 renderer 의 값이 바뀐다.
        /// 이 renderer 에서 사용하는 material 의 값만 바꾸고 싶으면
        /// 값을 바꾸기 전에
        /// sharing 을 멈춰야한다.
        /// </summary>
        public void BreakMaterialSharing()
        {
            _unmanagedMaterial = _material.internal_GetSource().MakeUnsharedCopy();
            _material = _unmanagedMaterial.internal_CreateReference();
            // note : material 이 삭제되지 않고, memory leak 의 위험이 있나?
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

            // todo : 새로 만들어진 material 일 경우 기본값 적용. 더 나은 방법을 찾아야한다.
            if (_material.internal_ReferenceCount == 1)
            {
                _material.asset.cullMode = CullMode.CullClockwiseFace;
                _material.asset.SetVector4("_BaseColor", new Vector4(1, 1, 1, 1));
            }

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

            }, material.asset.renderPriority);

            if (transform.IsHiddenInHierarchy == false)
            {
                if (_mesh != null && _mesh.asset != null)
                {
                    renderState.renderedVertexCount += _mesh.asset.v.Length;
                }
            }
        }

        public float? RaycastToBounds(Ray ray)
        {
            if (_mesh == null) return null;
            if (_mesh.asset == null) return null;

            // ray 를 local 좌표계로 변환
            var localRayOrigin = Vector3.Transform(ray.Position, transform.worldToLocalMatrix);
            var localRayDirection = Vector3.TransformNormal(ray.Direction, transform.worldToLocalMatrix);

            // _mesh.asset.bounds 와 교차 판정
            return _mesh.asset.bounds.Intersects(new Ray(localRayOrigin, localRayDirection));
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
