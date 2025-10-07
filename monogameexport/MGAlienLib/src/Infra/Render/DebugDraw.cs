using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MGAlienLib
{
    /// <summary>
    /// 디버깅용 그리기 함수를 제공합니다.
    /// </summary>
    public static class DebugDraw
    {
        private static InternalRenderManager renderer => GameBase.Instance.internalRenderManager;
        private static GraphicsDevice graphicsDevice => GameBase.Instance.GraphicsDevice;

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

        public class DebugMesh
        {
            public VertexPositionNormalTextureColor[] v;
            public short[] indices;
        }


        public static void internal_DrawLine(BasicEffect effect, Vector3 start, Vector3 end, Color color)
        {
            VertexPositionColor[] vertices =
            {
                new VertexPositionColor(start, color),
                new VertexPositionColor(end, color)
            };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
            }
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawLine(effect, start, end, color);
            });
        }

        public static void internal_DrawLineStrip(BasicEffect effect, List<Vector3> points, Color color)
        {
            if (points.Count < 2) return;
            VertexPositionColor[] vertices = new VertexPositionColor[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                // transformation 적용
                vertices[i] = new VertexPositionColor(points[i], color);
            }
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, points.Count - 1);
            }
        }

        public static void DrawLineStrip(List<Vector3> points, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawLineStrip(effect, points, color);
            });
        }

        public static void DrawRectangle(RectangleF rect, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawLineStrip(effect, new List<Vector3>()
                {
                    new Vector3(rect.Left, rect.Top, 0),
                    new Vector3(rect.Right, rect.Top, 0),
                    new Vector3(rect.Right, rect.Bottom, 0),
                    new Vector3(rect.Left, rect.Bottom, 0),
                    new Vector3(rect.Left, rect.Top, 0),
                }, color);
            });
        }

        private static void internal_DrawCircle(BasicEffect effect, Vector3 center, Vector3 Normal, float radius, Color color)
        {
            var distance = Vector3.Transform(center, effect.View).Length();
            if (distance < 1) distance = 1; // 1 이하로 가면 세그먼트 수가 너무 커지므로 1로 설정
            // 반지름에 따라 세그먼트 수를 조절
            float resolution = 400f / distance;
            int segments = (int)(radius * resolution);
            segments = Math.Max(segments, 16); // 최소 세그먼트 수를 16으로 설정
            segments = Math.Min(segments, 64); // 최대 세그먼트 수를 128로 설정
            float increment = MathHelper.TwoPi / segments;

            // Axis 정규화
            Vector3 normalizedAxis = Vector3.Normalize(Normal);

            // 기준 평면의 벡터 생성 (Axis와 수직인 두 벡터)
            Vector3 basisVector1;
            if (Math.Abs(Vector3.Dot(normalizedAxis, Vector3.UnitZ)) < 0.999f)
            {
                basisVector1 = Vector3.Normalize(Vector3.Cross(normalizedAxis, Vector3.UnitZ));
            }
            else
            {
                basisVector1 = Vector3.Normalize(Vector3.Cross(normalizedAxis, Vector3.UnitX));
            }
            Vector3 basisVector2 = Vector3.Cross(normalizedAxis, basisVector1);

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = increment * i;
                // 원의 기본 점 계산 (XY 평면에서)
                float x = (float)Math.Cos(angle) * radius;
                float y = (float)Math.Sin(angle) * radius;

                // Axis를 기준으로 새로운 점 생성
                Vector3 point = center + (basisVector1 * x) + (basisVector2 * y);
                points.Add(point);
            }

            internal_DrawLineStrip(effect, points, color);
        }

        public static void DrawCircle(Vector3 center, Vector3 Normal, float radius, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawCircle(effect, center, Normal, radius, color);
            });
        }

        public static void DrawArrow(Vector3 start, Vector3 end, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawLine(effect, start, end, color);

                Vector3 delta = end - start;
                Vector3 direction = delta.Normalized();

                float tooSmall = 0.01f;
                Quaternion rot1;
                Quaternion rot2;
                if (Mathf.Abs(direction.Y - 1) < tooSmall)
                {
                    rot1 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 150f.ToRadians());
                    rot2 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -150f.ToRadians());
                }
                else
                {
                    rot1 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 150f.ToRadians());
                    rot2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -150f.ToRadians());
                }

                Vector3 left = Vector3.Transform(direction, rot1) * (end - start).Length() * 0.1f;
                Vector3 right = Vector3.Transform(direction, rot2) * (end - start).Length() * 0.1f;

                internal_DrawLine(effect, end, end - (direction * 10) + left, color);
                internal_DrawLine(effect, end, end - (direction * 10) + right, color);
            });
        }

        public static void DrawBounding(BoundingBox box, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                Vector3[] corners = box.GetCorners();
                internal_DrawLine(effect, corners[0], corners[1], color);
                internal_DrawLine(effect, corners[1], corners[2], color);
                internal_DrawLine(effect, corners[2], corners[3], color);
                internal_DrawLine(effect, corners[3], corners[0], color);

                internal_DrawLine(effect, corners[4], corners[5], color);
                internal_DrawLine(effect, corners[5], corners[6], color);
                internal_DrawLine(effect, corners[6], corners[7], color);
                internal_DrawLine(effect, corners[7], corners[4], color);

                internal_DrawLine(effect, corners[0], corners[4], color);
                internal_DrawLine(effect, corners[1], corners[5], color);
                internal_DrawLine(effect, corners[2], corners[6], color);
                internal_DrawLine(effect, corners[3], corners[7], color);
            });
        }

        public static void DrawSphere(Vector3 center, float radius, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawCircle(effect, center, Vector3.UnitX, radius, color);
                internal_DrawCircle(effect, center, Vector3.UnitY, radius, color);
                internal_DrawCircle(effect, center, Vector3.UnitZ, radius, color);
            });
        }

        public static void internal_DrawMesh(BasicEffect effect, DebugMesh mesh)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, 
                    mesh.v, 0, mesh.v.Length, 
                    mesh.indices, 0, mesh.indices.Length / 3);
            }
        }

        public static void DrawMesh(DebugMesh mesh)
        {
            renderer.StackDrawCommand((effect) =>
            {
                internal_DrawMesh(effect, mesh);
            });
        }
    }
}
