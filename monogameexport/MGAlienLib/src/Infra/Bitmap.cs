
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// (WIP) 비트맵을 나타냅니다.
    /// 현재 1 channel 만 구현되어 있습니다.
    /// </summary>
    public class Bitmap
    {
        public int Width = 0;
        public int Height = 0;
        public int Channels = 1;
        public byte[] Data = null;

        public Bitmap(int width, int height, int channels)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Data = new byte[width * height * channels];
        }

        public Bitmap(int width, int height, int channels, byte[] externalData)
        {
            Width = width;
            Height = height;
            Channels = channels;
            if (externalData.Length != width * height * channels)
            {
                throw new Exception("External data size does not match bitmap size");
            }
            Data = new byte[width * height * channels];
            Array.Copy(externalData, Data, externalData.Length);
        }

        public void Fill(byte value)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = value;
            }
        }

        public void SetPixel(int x, int y, byte value)
        {
            int index = (y * Width + x) * Channels;
            Data[index] = value;
        }

        public byte GetPixel(int x, int y)
        {
            int index = (y * Width + x) * Channels;
            return Data[index];
        }

        public static void Blit(Bitmap dest, Rectangle destRect, Bitmap src, Rectangle srcRect)
        {
            for (int y = 0; y < destRect.Height; y++)
            {
                for (int x = 0; x < destRect.Width; x++)
                {
                    var destX = destRect.X + x;
                    var destY = destRect.Y + y;
                    var srcX = srcRect.X + x;
                    var srcY = srcRect.Y + y;

                    if (destX < 0 || destX >= dest.Width || destY < 0 || destY >= dest.Height)
                    {
                        continue;
                    }
                    if (srcX < 0 || srcX >= src.Width || srcY < 0 || srcY >= src.Height)
                    {
                        continue;
                    }

                    int destIndex = (destY * dest.Width + destX) * dest.Channels;
                    int srcIndex = (srcY * src.Width + srcX) * src.Channels;

                    //int destIndex = ((destRect.Y + y) * dest.Width + (destRect.X + x)) * dest.Channels;
                    //int srcIndex = ((srcRect.Y + y) * src.Width + (srcRect.X + x)) * src.Channels;

                    for (int c = 0; c < dest.Channels; c++)
                    {
                        dest.Data[destIndex + c] = src.Data[srcIndex + c];
                    }
                }
            }
        }

        public Texture2D ToTexture2D()
        {
            Texture2D texture = new Texture2D(GameBase.Instance.GraphicsDevice, Width, Height, false, SurfaceFormat.Alpha8);
            texture.SetData<byte>(Data);
            return texture;
        }
    }
}
