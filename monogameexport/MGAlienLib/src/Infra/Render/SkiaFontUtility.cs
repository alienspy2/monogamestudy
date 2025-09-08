
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace MGAlienLib
{
    public class SkiaFontUtility
    {
        private SKTypeface _typeface;
        private SKFont _font;
        private float? markerSize = null;

        public SkiaFontUtility(string fontName)
        {
            var rootPath = GameBase.Instance.assetManager.rawAssetsRootPath;
            string fontPath = @$"{rootPath}\font\{fontName}.ttf";
            //string fontPath = @"C:\playground\study\monogame\Project1\Project1\Assets\font\NotoSansKR-Regular.ttf";
            _typeface = SKTypeface.FromFile(fontPath, 0);

            if (_typeface == null)
            {
                // 폰트 로드 실패 시 처리
                Logger.Log("cannot load NotoSansKR font");
                return;
            }

            _font = new SKFont(_typeface);
            _font.Subpixel = false;
            _font.Edging = SKFontEdging.SubpixelAntialias;
            // _font.Edging = SKFontEdging.Alias;

        }

        public Texture2D RenderString(string str, float fontSize, out int yOffsetMin)
        {
            _font.Size = fontSize;

            using (var paint = new SKPaint())
            {
                markerSize ??= _font.MeasureText(".", paint);
                _font.MeasureText($".{str}.", out var bounds, paint);
                paint.Color = SKColors.White;

                yOffsetMin = (int)bounds.Top;

                AdjustBound(bounds, fontSize, out float width, out float height);

                var info = new SKImageInfo((int)(width + 2 + .5f), (int)(height + .5f), SKColorType.Gray8);

                using (var surface = SKSurface.Create(info))
                {
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Black);
                    //canvas.DrawText(str, -bounds.Left + margin, -bounds.Top + margin, notoKRFont, paint);
                    canvas.DrawText(str, -bounds.Left, -bounds.Top, _font, paint);

                    using (var image = surface.Snapshot())
                    {
                        var pixels = image.PeekPixels();
                        byte[] pixelData = pixels.GetPixelSpan().ToArray();
                        var device = GameBase.Instance.GraphicsDevice;

                        var texture = new Texture2D(device, info.Width, info.Height, true, SurfaceFormat.Alpha8);
                        texture.SetData(pixelData);
                        return texture;
                    }
                }

            }

        }

        public Vector2 GetSizeFromString(string str, float fontSize, out int yOffsetMin)
        {
            _font.Size = fontSize;

            using (var paint = new SKPaint())
            {
                markerSize ??= _font.MeasureText(".", paint);
                _font.MeasureText($".{str}.", out var bounds, paint);

                yOffsetMin = (int)bounds.Top;

                AdjustBound(bounds, fontSize, out float width, out float height);

                return new Vector2(width, height);
            }

        }

        private void AdjustBound(SKRect bounds, float fontSize, out float width, out float height)
        {
            float margin = 0;

            width = ((bounds.Width) + margin * 2 - markerSize.Value * 2f) + 3;
            float _height = (bounds.Height) + margin * 2;
            height = _height;// - (fontSize * 4 / 10);
            if (height < 3)
            {
                height = 3;
            }
        }
    }
}
