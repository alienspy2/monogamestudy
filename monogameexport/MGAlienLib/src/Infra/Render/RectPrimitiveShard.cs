
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 사각형 프리미티브를 나타내는 클래스입니다.
    /// PrimitiveBatch를 사용하여 효율적으로 그리기 위해 사용됩니다.
    /// Renderable 컴포넌트에서 사용됩니다.
    /// </summary>
    public class RectPrimitiveShard : PrimitiveShardBase
    {
        protected override int TrianglesCount => 2;
        protected override int VerticesCount => 4;

        public RectPrimitiveShard() : base()
        {
        }


        protected override void UpdatePos()
        {
            float w = _size.X, h = _size.Y;
            float px = _pivot.X * _size.X, py = _pivot.Y * _size.Y;
            float left = _offset.X - px, right = _offset.X + w - px, bottom = _offset.Y - py, top = _offset.Y + h - py;

            pos[0].X = left; pos[0].Y = bottom; pos[0].Z = 0; // bottom-left
            pos[1].X = left; pos[1].Y = top; pos[1].Z = 0;    // top-left
            pos[2].X = right; pos[2].Y = top; pos[2].Z = 0;   // top-right
            pos[3].X = right; pos[3].Y = bottom; pos[3].Z = 0;// bottom-right
        }

        protected override void UpdateVertices()
        {
            // 기본 UV 설정 (전체 텍스처 기준)
            Vector2 uvBottomLeft, uvTopLeft, uvTopRight, uvBottomRight;

            if (_sourceRect.HasValue && _textureSize.X > 0 && _textureSize.Y > 0)
            {
                // sourceRect를 기반으로 UV 좌표 계산
                Rectangle rect = _sourceRect.Value;
                float texWidth = _textureSize.X;
                float texHeight = _textureSize.Y;

                uvBottomLeft = new Vector2(rect.Left / texWidth, rect.Bottom / texHeight);
                uvTopLeft = new Vector2(rect.Left / texWidth, rect.Top / texHeight);
                uvTopRight = new Vector2(rect.Right / texWidth, rect.Top / texHeight);
                uvBottomRight = new Vector2(rect.Right / texWidth, rect.Bottom / texHeight);
            }
            else
            {
                // sourceRect가 없으면 전체 텍스처 사용
                uvBottomLeft = new Vector2(0f, 1f); // bottom-left
                uvTopLeft = new Vector2(0f, 0f);    // top-left
                uvTopRight = new Vector2(1f, 0f);   // top-right
                uvBottomRight = new Vector2(1f, 1f); // bottom-right
            }

            // FlipX에 따라 UV 조정
            if (_FlipX)
            {
                uvs[0] = uvBottomRight; // bottom-left -> bottom-right (X 반전)
                uvs[1] = uvTopRight;    // top-left -> top-right (X 반전)
                uvs[2] = uvTopLeft;     // top-right -> top-left (X 반전)
                uvs[3] = uvBottomLeft;  // bottom-right -> bottom-left (X 반전)
            }
            else
            {
                uvs[0] = uvBottomLeft;  // bottom-left
                uvs[1] = uvTopLeft;     // top-left
                uvs[2] = uvTopRight;    // top-right
                uvs[3] = uvBottomRight; // bottom-right
            }


            for(int i = 0; i < VerticesCount; i++)
            {
                // extData 설정
                extData[i] = new Vector4((float)_scissorsID, 0, 0, 0);
                // 정점 데이터 업데이트
                vstream[i] = new VertexPositionColorTextureExt(Vector3.Zero, _color, uvs[i], extData[i]);
            }
        }
    
        protected override void UpdateIndices(int baseIndex)
        {
            istream[0] = (short)(baseIndex + 0);
            istream[1] = (short)(baseIndex + 1);
            istream[2] = (short)(baseIndex + 2);
            istream[3] = (short)(baseIndex + 0);
            istream[4] = (short)(baseIndex + 2);
            istream[5] = (short)(baseIndex + 3);
        }
    }
}
