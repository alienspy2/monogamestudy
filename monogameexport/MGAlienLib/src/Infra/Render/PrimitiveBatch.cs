
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGAlienLib
{
    /// <summary>
    /// 기본 프리미티브를 그리는 클래스입니다.
    /// PrimitiveShard 를 모아서 한번에 그리는 방식으로 사용합니다.
    /// </summary>
    public class PrimitiveBatch
    {
        private GraphicsDevice device;
        private DepthStencilState depthState;
        private Material material;

        /// <summary>
        /// 새 PrimitiveBatch 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="material"></param>
        public PrimitiveBatch(Material material)
        {
            device = GameBase.Instance.GraphicsDevice;
            this.material = material;

            depthState = new DepthStencilState
            {
                DepthBufferEnable = true,    // zread 활성화 (깊이 테스트 수행)
                DepthBufferWriteEnable = true, // zwrite 활성화 (깊이 버퍼에 쓰기)
                DepthBufferFunction = CompareFunction.LessEqual // 깊이 비교 방식 (기본값: Less 또는 LessEqual)
            };
        }

        public void Draw<T>(RenderState renderState, Camera cam, RenderChunk chunk) where T : struct, IVertexType
        {
            if (chunk.vertexCount == 0 || chunk.indexCount == 0) return;

            material.ApplyParams();
            material.shader.effect.Parameters["ViewProjection"].SetValue(cam.matViewProjection);
            material.shader.effect.Parameters["World"]?.SetValue(Matrix.Identity);

            device.DepthStencilState = depthState;

            BlendState customBlend = new BlendState()
            {
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,  // 일반적인 Alpha Blending 방식
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            };

            device.BlendState = customBlend;

            foreach (EffectPass pass in material.shader.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                T[] vertices = chunk.vertexStream as T[];
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertices, 0, chunk.vertexCount,
                    chunk.indexStream, 0, chunk.indexCount/3);
            }
        }

    }
}
