using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 9-슬라이스 프리미티브를 나타내는 클래스입니다.
    /// 9개 영역으로 나누어진 텍스처를 사용하여 크기 조정이 가능한 UI 요소를 렌더링합니다.
    /// PrimitiveBatch를 사용하여 효율적으로 그리기 위해 사용됩니다.
    /// Renderable 컴포넌트에서 사용됩니다.
    /// </summary>
    public class NineSlicePrimitiveShard : PrimitiveShardBase
    {
        protected override int TrianglesCount => 18; // 9개 영역 * 2개 삼각형 = 18개 삼각형
        protected override int VerticesCount => 16;

        // 9-슬라이스 특정 속성
        private int _leftMargin = 16;
        private int _rightMargin = 16;
        private int _topMargin = 16;
        private int _bottomMargin = 16;

        /// <summary>
        /// 9-슬라이스의 왼쪽 마진을 가져오거나 설정합니다.
        /// </summary>
        public int leftMargin
        {
            get => _leftMargin;
            set
            {
                if (_leftMargin != value)
                {
                    _leftMargin = value;
                    vertexDirty = true;
                }
            }
        }

        /// <summary>
        /// 9-슬라이스의 오른쪽 마진을 가져오거나 설정합니다.
        /// </summary>
        public int rightMargin
        {
            get => _rightMargin;
            set
            {
                if (_rightMargin != value)
                {
                    _rightMargin = value;
                    vertexDirty = true;
                }
            }
        }

        /// <summary>
        /// 9-슬라이스의 위쪽 마진을 가져오거나 설정합니다.
        /// </summary>
        public int topMargin
        {
            get => _topMargin;
            set
            {
                if (_topMargin != value)
                {
                    _topMargin = value;
                    vertexDirty = true;
                }
            }
        }

        /// <summary>
        /// 9-슬라이스의 아래쪽 마진을 가져오거나 설정합니다.
        /// </summary>
        public int bottomMargin
        {
            get => _bottomMargin;
            set
            {
                if (_bottomMargin != value)
                {
                    _bottomMargin = value;
                    vertexDirty = true;
                }
            }
        }

        /// <summary>
        /// 모든 마진을 한번에 설정합니다.
        /// </summary>
        /// <param name="left">왼쪽 마진</param>
        /// <param name="top">위쪽 마진</param>
        /// <param name="right">오른쪽 마진</param>
        /// <param name="bottom">아래쪽 마진</param>
        public void SetMargins(int left, int top, int right, int bottom)
        {
            if (_leftMargin != left || _topMargin != top || _rightMargin != right || _bottomMargin != bottom)
            {
                _leftMargin = left;
                _topMargin = top;
                _rightMargin = right;
                _bottomMargin = bottom;
                vertexDirty = true;
            }
        }

        public NineSlicePrimitiveShard() : base()
        {
        }

        protected override void UpdatePos()
        {
            float w = _size.X, h = _size.Y;
            float px = _pivot.X * _size.X, py = _pivot.Y * _size.Y;

            // 9-슬라이스 그리드의 좌표 계산
            float left = _offset.X - px;
            float right = _offset.X + w - px;
            float bottom = _offset.Y - py;
            float top = _offset.Y + h - py;

            // 9개 영역을 위한 내부 분할점 계산
            float leftSplit = left + _leftMargin;
            float rightSplit = right - _rightMargin;
            float bottomSplit = bottom + _bottomMargin;
            float topSplit = top - _topMargin;

            pos[0] = new Vector3(left, top, 0);
            pos[1] = new Vector3(leftSplit, top, 0);
            pos[2] = new Vector3(rightSplit, top, 0);
            pos[3] = new Vector3(right, top, 0);

            pos[4] = new Vector3(left, topSplit, 0);
            pos[5] = new Vector3(leftSplit, topSplit, 0);
            pos[6] = new Vector3(rightSplit, topSplit, 0);
            pos[7] = new Vector3(right, topSplit, 0);

            pos[8] = new Vector3(left, bottomSplit, 0);
            pos[9] = new Vector3(leftSplit, bottomSplit, 0);
            pos[10] = new Vector3(rightSplit, bottomSplit, 0);
            pos[11] = new Vector3(right, bottomSplit, 0);

            pos[12] = new Vector3(left, bottom, 0);
            pos[13] = new Vector3(leftSplit, bottom, 0);
            pos[14] = new Vector3(rightSplit, bottom, 0);
            pos[15] = new Vector3(right, bottom, 0);
        }

        protected override void UpdateVertices()
        {
            // UV 좌표 계산
            CalculateUVs();

            // 확장 데이터 설정
            for (int i = 0; i < VerticesCount; i++)
            {
                extData[i] = new Vector4((float)_scissorsID, 0, 0, 0);
            }

            for (int i = 0; i < VerticesCount; i++)
            {
                // extData 설정
                extData[i] = new Vector4((float)_scissorsID, 0, 0, 0);
                // 정점 데이터 업데이트
                vstream[i] = new VertexPositionColorTextureExt(Vector3.Zero, _color, uvs[i], extData[i]);
            }

        }

        protected override void UpdateIndices(int baseIndex)
        {
            // 인덱스 스트림 초기화
            int idx = 0;

            // 4x4 그리드의 정점으로 이루어진 3x3 그리드의 셀을 순회
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    // 현재 셀의 왼쪽 상단 정점 인덱스 계산
                    int topLeft = baseIndex + row * 4 + col;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + 4;
                    int bottomRight = bottomLeft + 1;

                    bool ccw = true;

                    if (ccw)
                    {
                        // 첫 번째 삼각형 (반시계 방향)
                        istream[idx++] = (short)(topLeft);
                        istream[idx++] = (short)(topRight);
                        istream[idx++] = (short)(bottomLeft);

                        // 두 번째 삼각형 (반시계 방향)
                        istream[idx++] = (short)(topRight);
                        istream[idx++] = (short)(bottomRight);
                        istream[idx++] = (short)(bottomLeft);
                    }
                    else
                    {
                        // 첫 번째 삼각형 (시계 방향)
                        istream[idx++] = (short)(topLeft);
                        istream[idx++] = (short)(bottomLeft);
                        istream[idx++] = (short)(topRight);

                        // 두 번째 삼각형 (시계 방향)
                        istream[idx++] = (short)(topRight);
                        istream[idx++] = (short)(bottomLeft);
                        istream[idx++] = (short)(bottomRight);
                    }
                }
            }
        }

        private void CalculateUVs()
        {
            Rectangle sourceRectangle;
            if (_sourceRect.HasValue)
            {
                sourceRectangle = _sourceRect.Value;
            }
            else
            {
                sourceRectangle = new Rectangle(0, 0, (int)_textureSize.X, (int)_textureSize.Y);
            }

            float texWidth = _textureSize.X;
            float texHeight = _textureSize.Y;

            if (texWidth <= 0 || texHeight <= 0)
            {
                texWidth = 1;
                texHeight = 1;
            }

            // 소스 영역에서 9-슬라이스 UV 좌표 계산
            float leftU = sourceRectangle.Left / texWidth;
            float rightU = sourceRectangle.Right / texWidth;
            float topV = sourceRectangle.Top / texHeight;
            float bottomV = sourceRectangle.Bottom / texHeight;

            // 9-슬라이스 내부 분할 UV 좌표
            float leftSplitU = (sourceRectangle.Left + _leftMargin) / texWidth;
            float rightSplitU = (sourceRectangle.Right - _rightMargin) / texWidth;
            float topSplitV = (sourceRectangle.Top + _topMargin) / texHeight;
            float bottomSplitV = (sourceRectangle.Bottom - _bottomMargin) / texHeight;

            if (_FlipX)
            {
                // X축 반전 시 UV 좌표 조정
                float temp = leftU;
                leftU = rightU;
                rightU = temp;

                temp = leftSplitU;
                leftSplitU = rightSplitU;
                rightSplitU = temp;
            }

            uvs[0] = new Vector2(leftU, topV); // top-left
            uvs[1] = new Vector2(leftSplitU, topV); // top-left split
            uvs[2] = new Vector2(rightSplitU, topV); // top-right split
            uvs[3] = new Vector2(rightU, topV); // top-right

            uvs[4] = new Vector2(leftU, topSplitV); // top-left split
            uvs[5] = new Vector2(leftSplitU, topSplitV); // top-left split
            uvs[6] = new Vector2(rightSplitU, topSplitV); // top-right split
            uvs[7] = new Vector2(rightU, topSplitV); // top-right split

            uvs[8] = new Vector2(leftU, bottomSplitV); // bottom-left split
            uvs[9] = new Vector2(leftSplitU, bottomSplitV); // bottom-left split
            uvs[10] = new Vector2(rightSplitU, bottomSplitV); // bottom-right split
            uvs[11] = new Vector2(rightU, bottomSplitV); // bottom-right split

            uvs[12] = new Vector2(leftU, bottomV); // bottom-left
            uvs[13] = new Vector2(leftSplitU, bottomV); // bottom-left split
            uvs[14] = new Vector2(rightSplitU, bottomV); // bottom-right split
            uvs[15] = new Vector2(rightU, bottomV); // bottom-right
        }

        
    }
}