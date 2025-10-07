

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MGAlienLib
{
    public class Mesh : MGDisposableObject
    {
        private static int _serialNumberSeed = 0;
        public int serialNumber { get; private set; } = _serialNumberSeed++;
        public string id => $"{serialNumber}";

        [StructLayout(LayoutKind.Sequential, Pack = 1)] // 메모리 정렬 최적화
        public struct VertexPositionNormalTextureColor : IVertexType
        {
            public Vector3 Position;    // P
            public Vector3 Normal;      // N
            public Vector2 TexCoord;    // T
            public Color Color;         // C

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                // Position (float3)  -> 12 bytes
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),

                // Normal (float3) -> 12 bytes
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),

                // TexCoord (float2) -> 8 bytes
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                // Color (RGBA8) -> 4 bytes
                new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texCoord, Color color)
            {
                Position = position;
                Normal = normal;
                TexCoord = texCoord;
                Color = color;
            }
        }

        public VertexPositionNormalTextureColor[] v;
        public short[] indices;
        public BoundingBox bounds;

        public void CalculateBounds()
        {
            Vector3[] points = v.Select(vertex => vertex.Position).ToArray();
            bounds = BoundingBox.CreateFromPoints(points);
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //_asset?.Dispose();
                }
                //_asset = null;
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Mesh()
        {
            Dispose(false);
        }
        #endregion
    }
}
