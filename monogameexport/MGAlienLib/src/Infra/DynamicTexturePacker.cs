using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 동적으로 Texture 를 Packing 하는 클래스입니다.
    /// DynamicRectPacker 를 사용해서 Texture2D 를 Packing 합니다.
    /// 여러 페이지를 가질 수 있습니다.
    /// (WIP) page 의 coverage 가 일정 수준 이하로 떨어지면 repack 을 시도합니다. (현재 버그로 인해 repack 이 제대로 동작하지 않습니다.)
    /// texture format 은 SurfaceFormat.Color 와 SurfaceFormat.Alpha8 를 지원합니다.
    /// </summary>
    public class DynamicTexturePacker
    {
        private string _name = "DynamicTexturePacker";
        private float _repackCoverage = 0.8f;
        private int _rejectDiff = 10;
        private int _pageSize = 2048;
        private int _padding = 0;
        private int _IdSeed = 0;
        private SurfaceFormat _surfaceFormat;

        private List<(DynamicRectPacker packer, RenderTarget2D texture)> pages = new();

        private Texture2D transparentTexture;
        private Texture2D transparentTextureAlpha8;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="name"></param>
        /// <param name="format"></param>
        /// <param name="pageSize"></param>
        /// <param name="padding"></param>
        public DynamicTexturePacker(string name, SurfaceFormat format, int pageSize = 2048, int padding = 2)
        {
            _name = name;
            _pageSize = pageSize;
            _padding = padding;
            _IdSeed = 0;
            _surfaceFormat = format;
            transparentTexture = new Texture2D(GameBase.Instance.GraphicsDevice, 1, 1);
            transparentTexture.SetData(new[] { Color.Transparent });
            transparentTextureAlpha8 = new Texture2D(GameBase.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Alpha8);
            transparentTextureAlpha8.SetData(new byte[] { 0 });
        }

        /// <summary>
        /// Texture2D 를 Packing 합니다.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rect"></param>
        /// <param name="repacked"></param>
        /// <returns></returns>
        public int Insert(Texture2D source, bool dialate, out Rectangle rect, out bool repacked)
        {
            rect = Rectangle.Empty;
            repacked = false;

            _IdSeed++;
            int Id = _IdSeed;

            bool success = false;
            int pageIndex = -1;
            for (int i = 0; i < pages.Count; i++)
            {
                success = TryInsertToPage(i, source, Id, out rect, out repacked);
                if (success)
                {
                    pageIndex = i;
                    break;
                }
            }

            // 3 실패시, page 추가
            if (success == false)
            {
                AddPage();
                success = TryInsertToPage(pages.Count - 1, source, Id, out rect, out repacked);
                if (success)
                {
                    pageIndex = pages.Count - 1;
                }
            }

            if (success)
            {
                var device = GameBase.Instance.GraphicsDevice;
                device.SetRenderTarget(pages[pageIndex].texture);
                var spriteBatch = new SpriteBatch(device);
                spriteBatch.Begin();
                if (dialate)
                {
                    var rectDialate = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
                    spriteBatch.Draw(source, rectDialate, Color.White);
                }
                spriteBatch.Draw(source, rect, Color.White);
                spriteBatch.End();
                device.SetRenderTarget(null);

                return Id;
            }

            Logger.Log("insert failed");

            rect = Rectangle.Empty;
            return -1;
        }

        private void AddPage()
        {
            var newTex = new RenderTarget2D(GameBase.Instance.GraphicsDevice, _pageSize, _pageSize, false, _surfaceFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            newTex.Name = $"DTP:{_name}_{pages.Count}";
            pages.Add(
                    (new DynamicRectPacker(_pageSize, _pageSize, _padding),
                    newTex)
                );
        }

        private bool TryInsertToPage(int pageIndex, Texture2D source, int Id, out Rectangle rect, out bool repacked)
        {
            repacked = false;
            rect = Rectangle.Empty;

            var packer = pages[pageIndex].packer;

            // 1. 이 page 에 추가 시도
            bool success = packer.TryInsert(source.Width, source.Height, Id, out var _rect1);

            // 실패시, coverage 가 작으면 repack
            if (success == true)
            {
                rect = _rect1;
            }

            if (success == false)
            {
                bool allowRepack = false;
                // todo : repack 이 제대로 동작하지 않음. 수정필요
                if (allowRepack && (packer.coverage < _repackCoverage))
                {
                    // 2 repack
                    var newPacker = new DynamicRectPacker(packer.width, packer.height, _padding, _rejectDiff);
                    if (newPacker.RepackFrom(packer))
                    {
                        repacked = true;
                        if (newPacker.TryInsert(source.Width, source.Height, Id, out var _rect2))
                        {
                            rect = _rect2;
                            success = true;
                            var newTex = Migration(pageIndex, newPacker, packer);
                            pages[pageIndex] = (newPacker, newTex);
                        }
                    }
                }
            }

            return success;
        }

        private int testCount = 0;
        private RenderTarget2D Migration(int pageIndex, DynamicRectPacker newPacker, DynamicRectPacker oldPacker)
        {
            var newTex = new RenderTarget2D(GameBase.Instance.GraphicsDevice, newPacker.width, newPacker.height, false, _surfaceFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            var oldTex = pages[pageIndex].texture;

            newTex.Name = $"+{oldTex.Name}";

            //Utility.TextureUtility.SavePng(oldTex, $"old_{pageIndex}_{testCount}.png");

            var device = GameBase.Instance.GraphicsDevice;
            var spriteBatch = new SpriteBatch(device);
            device.SetRenderTarget(newTex);
            spriteBatch.Begin();

            var allItems = oldPacker.GetRects();
            foreach(var item in allItems)
            {
                if (item.Id == -1) continue;

                var srcTex = oldTex;
                var srcRect = item.rect;
                var destRect = newPacker.GetRect(item.Id);
                spriteBatch.Draw(srcTex, new Vector2(destRect.Value.X, destRect.Value.Y), srcRect, Color.White);
            }

            spriteBatch.End();
            device.SetRenderTarget(null);
            oldTex.Dispose();

            //Utility.TextureUtility.SavePng(newTex, $"new_{pageIndex}_{testCount}.png");

            testCount++;
            return newTex;
        }

        /// <summary>
        /// 디버깅용. 현재 packing 된 texture 를 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        public void debug_SavePng(string path)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                var stream = new System.IO.FileStream($"{path}_{i}.png", System.IO.FileMode.Create);
                {
                    var tex = pages[i].texture;
                    tex.SaveAsPng(stream, tex.Width, tex.Height);
                }
            }
        }

        /// <summary>
        /// atlasId 가 포함된 page index 를 반환합니다.
        /// </summary>
        /// <param name="atalsId"></param>
        /// <returns></returns>
        public int GetPage(int atalsId)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i].packer.Contains(atalsId)) return i;
            }

            return -1;
        }

        /// <summary>
        /// atlasId 에 해당하는 rect 를 반환합니다.
        /// </summary>
        /// <param name="atalsId"></param>
        /// <returns></returns>
        public Rectangle? GetRect(int atalsId)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                var rect = pages[i].packer.GetRect(atalsId);
                if (rect.HasValue)
                {
                    return rect.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// atlasId 에 해당하는 texture 를 반환합니다.
        /// </summary>
        /// <param name="atalsId"></param>
        /// <returns></returns>
        public RenderTarget2D GetTexture(int atalsId)
        {
            return pages[GetPage(atalsId)].texture;
        }

        /// <summary>
        /// pageIndex 에 해당하는 texture 를 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RenderTarget2D GetTexureByPageIndex(int index)
        {
            return pages[index].texture;
        }

        /// <summary>
        /// atlasId 에 해당하는 texture 를 제거합니다.
        /// </summary>
        /// <param name="atlasId"></param>
        /// <returns></returns>
        public Rectangle Remove(int atlasId)
        {
            int pageIndex = GetPage(atlasId);
            var packer = pages[pageIndex].packer;
            Rectangle rect = packer.GetRect(atlasId).Value;
            packer.TryRemove(atlasId);

            //Utility.TextureUtility.SavePng(pages[pageIndex].texture, $"before_remove_{atlasId}.png");

            var device = GameBase.Instance.GraphicsDevice;

            var spriteBatch = new SpriteBatch(device);
            device.SetRenderTarget(pages[pageIndex].texture);

            //device.BlendState = BlendState.AlphaBlend;

            spriteBatch.Begin();
            //Logger.Log(rect);

            // rect 영역을 투명으로 칠함
            if (_surfaceFormat == SurfaceFormat.Alpha8)
                spriteBatch.Draw(transparentTextureAlpha8, rect, Color.White);
            else
                spriteBatch.Draw(transparentTexture, rect, Color.White * 0);

            spriteBatch.End();
            device.SetRenderTarget(null);

            //Utility.TextureUtility.SavePng(pages[pageIndex].texture, $"after_remove_{atlasId}.png");

            return rect;
        }
    }
}
