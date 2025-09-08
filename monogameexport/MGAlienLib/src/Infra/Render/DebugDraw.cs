using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 디버깅용 그리기 함수를 제공합니다.
    /// </summary>
    public static class DebugDraw
    {
        private static InternalRenderManager renderer => GameBase.Instance.internalRenderManager;
        private static GraphicsDevice graphicsDevice => GameBase.Instance.GraphicsDevice;


        public static void _DrawLine(BasicEffect effect, Vector3 start, Vector3 end, Color color)
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
                _DrawLine(effect, start, end, color);
            });
        }

        public static void _DrawLineStrip(BasicEffect effect, List<Vector3> points, Color color)
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
        public static void DrawLines(List<Vector3> points, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                _DrawLineStrip(effect, points, color);
            });
        }

        public static void DrawRectangle(RectangleF rect, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                _DrawLineStrip(effect, new List<Vector3>()
                {
                    new Vector3(rect.Left, rect.Top, 0),
                    new Vector3(rect.Right, rect.Top, 0),
                    new Vector3(rect.Right, rect.Bottom, 0),
                    new Vector3(rect.Left, rect.Bottom, 0),
                    new Vector3(rect.Left, rect.Top, 0),
                }, color);
            });
        }

        private static void _DrawCircle(BasicEffect effect, Vector3 center, Vector3 Normal, float radius, Color color)
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

            _DrawLineStrip(effect, points, color);
        }

        public static void DrawCircle(Vector3 center, Vector3 Normal, float radius, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                _DrawCircle(effect, center, Normal, radius, color);
            });
        }

        public static void DrawArrow(Vector3 start, Vector3 end, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                _DrawLine(effect, start, end, color);

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

                _DrawLine(effect, end, end - (direction * 10) + left, color);
                _DrawLine(effect, end, end - (direction * 10) + right, color);
            });
        }

        public static void DrawBounding(BoundingBox box, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                Vector3[] corners = box.GetCorners();
                _DrawLine(effect, corners[0], corners[1], color);
                _DrawLine(effect, corners[1], corners[2], color);
                _DrawLine(effect, corners[2], corners[3], color);
                _DrawLine(effect, corners[3], corners[0], color);

                _DrawLine(effect, corners[4], corners[5], color);
                _DrawLine(effect, corners[5], corners[6], color);
                _DrawLine(effect, corners[6], corners[7], color);
                _DrawLine(effect, corners[7], corners[4], color);

                _DrawLine(effect, corners[0], corners[4], color);
                _DrawLine(effect, corners[1], corners[5], color);
                _DrawLine(effect, corners[2], corners[6], color);
                _DrawLine(effect, corners[3], corners[7], color);
            });
        }

        public static void DrawSphere(Vector3 center, float radius, Color color)
        {
            renderer.StackDrawCommand((effect) =>
            {
                _DrawCircle(effect, center, Vector3.UnitX, radius, color);
                _DrawCircle(effect, center, Vector3.UnitY, radius, color);
                _DrawCircle(effect, center, Vector3.UnitZ, radius, color);
            });
        }


        // todo
        //public static void _DrawText(SpriteBatch spriteBatch, TrueTypeSharpUtility font, string text, Vector2 position, Color color, HAlign hAlign = HAlign.center, VAlign valign = VAlign.center)
        //{
        //    font.DrawString(spriteBatch, position, text, 20, color, 0, 0, hAlign, valign);
        //}
    }
}
