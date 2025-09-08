//#region License
///* TrueTypeSharp
//   Copyright (c) 2010 Illusory Studios LLC

//   TrueTypeSharp is available at zer7.com. It is a C# port of Sean Barrett's
//   C library stb_truetype, which was placed in the public domain and is
//   available at nothings.org.

//   Permission to use, copy, modify, and/or distribute this software for any
//   purpose with or without fee is hereby granted, provided that the above
//   copyright notice and this permission notice appear in all copies.

//   THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//   WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
//   MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
//   ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
//   WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
//   ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
//   OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//*/
//#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using MGAlienLib;


namespace TrueTypeSharp
{
    /// <summary>
    /// Utility class for rendering TrueTypeFont
    /// </summary>
    public class TrueTypeSharpUtility
    {
        Dictionary<Tuple<int, char>, Tuple<Bitmap, int,int>> cache = new ();
        private TrueTypeFont ttf;
        private float spacing = 0f;

        /// <summary>
        /// Create a new instance of TrueTypeSharpUtility
        /// </summary>
        /// <param name="fontname"></param>
        public TrueTypeSharpUtility(string fontname)
        {
            ttf = new TrueTypeFont(fontname);
        }

        /// <summary>
        /// cache 를 모두 지운다
        /// </summary>
        public void ClearCache()
        {
            cache.Clear();
        }

        private Bitmap RenderChar(char ch, int fontSize, out int xOffset, out int yOffset)
        {
            if (cache.TryGetValue(new Tuple<int, char>(fontSize, ch), out Tuple<Bitmap, int, int> cached))
            {
                xOffset = cached.Item2;
                yOffset = cached.Item3;
                return cached.Item1;
            }

            float scale = ttf.GetScaleForPixelHeight(fontSize);
            byte[] data = ttf.GetCodepointBitmap(ch, scale, scale,
                out int width, out int height, out xOffset, out yOffset);

            try
            {
                var bitmap = new Bitmap(width, height, 1, data);

                cache.Add(new Tuple<int, char>(fontSize, ch), 
                    new Tuple<Bitmap, int, int>(bitmap, xOffset, yOffset));
                return bitmap;
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Logger.Log($"Use of code not included in the font : {ch}({(int)ch})");
            }

            return null;
        }

        /// <summary>
        /// string 을 렌더링하여 텍스처로 반환한다
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fontSize"></param>
        public Bitmap RenderString(string str, int fontSize, out int yOffsetMin)
        {
            Vector2 pos = Vector2.Zero;
            var size = GetSizeFromString(str, fontSize, out yOffsetMin);

            Bitmap stringBitmap = new Bitmap((int)(size.X + spacing * str.Length), (int)size.Y - yOffsetMin, 1);

            foreach (var ch in str)
            {
                if (ch == ' ')
                {
                    pos.X += fontSize / 4;
                    continue;
                }

                var bitmap = RenderChar(ch, fontSize, out int xOffset, out int yOffset);
                if (bitmap == null)
                {
                    continue;
                }

                // blit
                Bitmap.Blit(stringBitmap, 
                    new Rectangle((int)pos.X + xOffset, (int)pos.Y - yOffsetMin + yOffset, bitmap.Width, bitmap.Height), 
                    bitmap, 
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                pos.X += bitmap.Width + xOffset + spacing;
            }

            return stringBitmap;
        }


        /// <summary>
        /// string 을 render 했을 떄의 크기를 반환한다
        /// 실제로 render 하기 전에 크기를 알고 싶을 때 사용
        /// 하지만, ttf library 의 제약으로, 실제로 렌더하고 이를 cache 에 넣기 때문에,
        /// 그다지 효율적이지는 않다
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fontSize"></param>
        /// <param name="yOffsetMin"></param>
        /// <returns></returns>
        public Vector2 GetSizeFromString(string str, int fontSize, out int yOffsetMin)
        {
            bool first = true;
            Vector2 size = Vector2.Zero;
            yOffsetMin = 0; // avoid compile error
            foreach (var ch in str)
            {
                if (ch == ' ')
                {
                    size.X += fontSize / 4;
                    continue;
                }
                var bitmap = RenderChar(ch, fontSize, out int xOffset, out int yOffset);
                if (bitmap == null)
                {
                    continue;
                }
                size.X += bitmap.Width + xOffset + spacing;

                // todo : 적절한 값을 찾아야한다.
                var adjustedHeight = bitmap.Height - (fontSize * 6 / 10);
                if (adjustedHeight<3)
                {
                    adjustedHeight = 3;
                }

                size.Y = Math.Max(size.Y, adjustedHeight);
                if (first)
                {
                    yOffsetMin = yOffset;
                    first = false;
                }
                else yOffsetMin = Math.Min(yOffsetMin, yOffset);
            }

            return size;
        }

    }
}
