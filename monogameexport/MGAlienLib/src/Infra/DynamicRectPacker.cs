using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// rectangle 을 동적으로 packing 하는 클래스
    /// 실시간으로 넣고 빼는 것을 지원
    /// </summary>
    public class DynamicRectPacker
    {
        /// <summary>
        /// packer 내에서의 하나의 노드
        /// 이 아래에 rectangle 들이 들어감
        /// </summary>
        private class SkyLine
        {
            public int x; // packer 내에서의 이 노드의 x 좌표
            public int width; // 이 노드가 차지하는 너비
            public int height; // 이 노드가 차지하는 높이. packer의 높이와 같음
            public int cursorY; // 다음 payload를 넣을 수 있는 y 좌표
            public List<(int Id, Rectangle rect)> payloads = new();
            private List<Rectangle> freeRects = new(); // 무효화된 공간 캐싱
            public int remainHeight => height - cursorY; // 빈 공간
            public int remainArea => width * remainHeight; // 빈 공간의 넓이

            /// <summary>
            /// 새로운 rect 를 넣을 수 있는지 시도
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="itemWidth"></param>
            /// <param name="itemHeight"></param>
            /// <param name="Id"></param>
            /// <param name="rect"></param>
            /// <returns></returns>
            public bool TryInsert(DynamicRectPacker owner, int itemWidth, int itemHeight, int Id, ref Rectangle rect)
            {
                int paddedWidth = itemWidth + owner._padding * 2;
                int paddedHeight = itemHeight + owner._padding * 2;

                if (this.width < paddedWidth) return false;

                // 캐싱된 freeRects에서 사용 가능한 공간 찾기
                for (int i = 0; i < freeRects.Count; i++)
                {
                    Rectangle r = freeRects[i];
                    if (itemWidth <= r.Width && itemHeight <= r.Height)
                    {
                        rect = new Rectangle(r.X, r.Y, itemWidth, itemHeight);
                        freeRects.RemoveAt(i);
                        payloads.Add((Id, rect));
                        return true;
                    }
                }

                if (this.remainHeight < paddedHeight) return false;

                int paddedX = x + owner._padding;
                int paddedY = cursorY + owner._padding;

                rect = new Rectangle(paddedX, paddedY, itemWidth, itemHeight);
                payloads.Add((Id, rect));
                cursorY += paddedHeight;

                return true;
            }

            /// <summary>
            /// Id에 해당하는 rect 제거
            /// </summary>
            /// <param name="Id"></param>
            /// <returns></returns>
            public bool TryRemove(int Id)
            {
                for (int i = 0; i < payloads.Count; i++)
                {
                    if (payloads[i].Id == Id)
                    {
                        var payload = payloads[i];
                        payloads[i] = (-1, payload.rect);
                        freeRects.Add(payload.rect); // 제거된 공간 캐싱
                        return true;
                    }
                }
                return false;
            }
        }

        private int _width;
        private int _remainWidth;
        private int _height;
        private int _padding;
        private int _rejectWidthDiff;
        private List<SkyLine> _skylines;
        private float _coverage;
        private bool _isCoverageDirty = true;
        private bool _isGetRectsDirty = true;

        /// <summary>
        /// packer의 너비 얻기
        /// </summary>
        public int width => _width;
        /// <summary>
        /// packer의 높이 얻기
        /// </summary>
        public int height => _height;

        /// <summary>
        /// 현재 packer의 사용된 넓이의 비율
        /// </summary>
        public float coverage
        {
            get
            {
                if (_isCoverageDirty)
                {
                    _coverage = GetCoverage();
                    _isCoverageDirty = false;
                }

                return _coverage;
            }
        }

        private readonly List<(int Id, Rectangle rect)> _getRectsCachedResults = new(); // GetAll용 캐싱
        private HashSet<int> _IdSet = new();

        /// <summary>
        /// Id가 포함되어 있는지 확인
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool Contains(int Id) => _IdSet.Contains(Id);

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="width">넓이</param>
        /// <param name="height">높이</param>
        /// <param name="padding">rect 간 띄우기</param>
        /// <param name="rejectWidthDiff">추가할 rect 의 width가 skyline의 넓이 보다 이 값이하로 작으면 그 node 에는 추가하지 않는다. 값이 너무 크거나 너무 작으면 효율이 떨어진다. 10이 적당하다. </param>
        public DynamicRectPacker(int width, int height, int padding, int rejectWidthDiff = 10)
        {
            _width = width;
            _remainWidth = width;
            _height = height;
            _padding = padding;
            _skylines = new List<SkyLine>();
            _rejectWidthDiff = rejectWidthDiff;
        }

        private float GetCoverage()
        {
            int totalArea = _width * _height;
            int usedArea = 0;
            for (int i = 0; i < _skylines.Count; i++)
            {
                var node = _skylines[i];
                for (int j = 0; j < node.payloads.Count; j++)
                {
                    var payload = node.payloads[j];
                    if (payload.Id == -1) continue;
                    usedArea += (payload.rect.Width + _padding) * (payload.rect.Height + _padding);
                }
            }
            return (float)usedArea / totalArea;
        }

        /// <summary>
        /// rect 추가 시도
        /// </summary>
        /// <param name="itemWidth"></param>
        /// <param name="itemHeight"></param>
        /// <param name="Id"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool TryInsert(int itemWidth, int itemHeight, int Id, out Rectangle rect)
        {
            rect = Rectangle.Empty;

            var fittestNodes = FindFittestNode(itemWidth, itemHeight);
            for (int i = 0; i < fittestNodes.Count; i++)
            {
                if (fittestNodes[i].TryInsert(this, itemWidth, itemHeight, Id, ref rect))
                {
                    _isCoverageDirty = true;
                    _isGetRectsDirty = true;
                    _IdSet.Add(Id);
                    return true;
                }
            }

            int paddedWidth = itemWidth + _padding * 2;
            if (_remainWidth < paddedWidth) return false;

            var newNode = new SkyLine
            {
                x = _width - _remainWidth,
                width = paddedWidth,
                height = _height,
                cursorY = 0
            };

            if (newNode.TryInsert(this, itemWidth, itemHeight, Id, ref rect))
            {
                _remainWidth -= newNode.width;
                _skylines.Add(newNode);
                _isCoverageDirty = true;
                _isGetRectsDirty = true;
                _IdSet.Add(Id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// rect 제거 시도
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool TryRemove(int Id)
        {
            if (_IdSet.Contains(Id) == false) return false;

            for (int i = 0; i < _skylines.Count; i++)
            {
                if (_skylines[i].TryRemove(Id))
                {
                    _isCoverageDirty = true;
                    _isGetRectsDirty = true;
                    _IdSet.Remove(Id);
                    return true;
                }
            }
            return false;
        }

        private List<SkyLine> FindFittestNode(int itemWidth, int itemHeight)
        {
            int paddedItemWidth = itemWidth + _padding * 2;
            int paddedItemHeight = itemHeight + _padding * 2;

            var fittestNodes = new List<SkyLine>(_skylines.Count);
            for (int i = 0; i < _skylines.Count; i++)
            {
                var node = _skylines[i];
                int widthDiff = node.width - paddedItemWidth;
                if (node.width >= paddedItemWidth &&
                    node.remainHeight >= paddedItemHeight &&
                    widthDiff < _rejectWidthDiff)
                {
                    fittestNodes.Add(node);
                }
            }

            fittestNodes.Sort((a, b) => (a.width - paddedItemWidth).CompareTo(b.width - paddedItemWidth));
            return fittestNodes;
        }

        /// <summary>
        /// 모든 rect 얻기
        /// </summary>
        /// <returns></returns>
        public List<(int Id, Rectangle rect)> GetRects()
        {
            if (_isGetRectsDirty)
            {
                _getRectsCachedResults.Clear();
                for (int i = 0; i < _skylines.Count; i++)
                {
                    _getRectsCachedResults.AddRange(_skylines[i].payloads);
                }
                _isGetRectsDirty = false;
            }

            return _getRectsCachedResults;
        }

        /// <summary>
        /// Id에 해당하는 rect 얻기
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Rectangle? GetRect(int id)
        {
            var items = GetRects();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    return items[i].rect;
                }
            }
            return null;
        }

        /// <summary>
        /// 다른 packer의 모든 rect 를 이 packer 에 모두 추가
        /// </summary>
        /// <param name="oldPacker"></param>
        /// <returns></returns>
        public bool RepackFrom(DynamicRectPacker oldPacker)
        {
            var items = oldPacker.GetRects();
            if (items.Count == 0) return true;

            var itemArray = new (int Id, Rectangle rect)[items.Count];
            int validCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id != -1)
                {
                    itemArray[validCount++] = items[i];
                }
            }

            Array.Resize(ref itemArray, validCount);
            Array.Sort(itemArray, (a, b) => b.rect.Width.CompareTo(a.rect.Width));

            for (int i = 0; i < itemArray.Length; i++)
            {
                var item = itemArray[i];
                if (!TryInsert(item.rect.Width, item.rect.Height, item.Id, out _))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 디버깅용. packer 내의 모든 rect 를 그려서 png 로 저장
        /// </summary>
        /// <param name="filename"></param>
        public void debug_Visualize(string filename)
        {
            var device = GameBase.Instance.GraphicsDevice;
            var rt = new RenderTarget2D(device, _width, _height, false, SurfaceFormat.Color, DepthFormat.None);
            var spriteBatch = new SpriteBatch(device);
            device.SetRenderTarget(rt);
            device.Clear(Color.Transparent);
            spriteBatch.Begin();

            Texture2D pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });

            var items = GetRects();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var rect = item.rect;
                Color color = item.Id == -1 ? Color.Gray : Color.Red;
                spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
                spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color);
                spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
                spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color);
                spriteBatch.Draw(pixel, rect, color * 0.2f);
            }

            for (int i = 0; i < _skylines.Count; i++)
            {
                var node = _skylines[i];
                spriteBatch.Draw(pixel, new Rectangle(node.x, node.cursorY, node.width, 1), Color.Green);
                spriteBatch.Draw(pixel, new Rectangle(node.x, node.cursorY + node.remainHeight - 1, node.width, 1), Color.Green);
                spriteBatch.Draw(pixel, new Rectangle(node.x, node.cursorY, 1, node.remainHeight), Color.Green);
                spriteBatch.Draw(pixel, new Rectangle(node.x + node.width - 1, node.cursorY, 1, node.remainHeight), Color.Green);

                //spriteBatch.DrawString(GameBase.Instance.defaultAssets.debugFont, $"{node.remainHeight}", new Vector2(node.x, node.cursorY), Color.White);
            }

            spriteBatch.End();
            device.SetRenderTarget(null);

            using (var fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                rt.SaveAsPng(fs, _width, _height);
            }

            pixel.Dispose();
            rt.Dispose();
        }
    }
}
