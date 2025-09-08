using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace MGAlienLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)] // 메모리 정렬 최적화
    public struct VertexPositionColorTextureExt : IVertexType
    {
        private static readonly int ExtDataUsageChannelID = 2;

        public Vector3 Position;   // 위치
        public Color Color;        // 색상 (압축된 4바이트)
        public Vector2 TexCoord;   // 기본 텍스처 좌표
        public Vector4 ExtData;    // 추가 데이터 (float4)

        // 정점 선언 (Vertex Declaration)
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, ExtDataUsageChannelID) // extData 추가
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        // 생성자
        public VertexPositionColorTextureExt(Vector3 position, Color color, Vector2 texCoord, Vector4 extData)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
            ExtData = extData;
        }
    }
}
