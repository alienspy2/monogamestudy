
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace MGAlienLib.Utility
{
    public static class TextureUtility
    {
        public static void SavePng(Texture2D texture, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }

    }
}
