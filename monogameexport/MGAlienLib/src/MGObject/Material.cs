using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// (WIP) Material class
    /// </summary>
    public class Material : MGDisposableObject
    {
        private static int _serialNumberSeed = 0;
        public int renderPriority;
        public Shader shader;
        public int serialNumber { get; private set; } = _serialNumberSeed++;

        public string id => $"{serialNumber}:{shader.name}";

        public Dictionary<string, float> floatParams = new();
        public Dictionary<string, Vector4> vectorParams = new();    
        public Dictionary<string, Texture2D> textureParams = new();

        RasterizerState rasterizerState = new RasterizerState();
        public CullMode cullMode
        {
            get => rasterizerState.CullMode;
            set => rasterizerState.CullMode = value;
        }

        public void SetFloat(string name, float value)
        {
            floatParams[name] = value;
        }

        public void SetVector4(string name, Vector4 value)
        {
            vectorParams[name] = value;
        }

        public void SetTexture(string name, Texture2D value)
        {
            textureParams[name] = value;
        }

        public void ApplyParams()
        {
            foreach (var kv in floatParams)
            {
                shader.SetFloat(kv.Key, kv.Value);
            }

            foreach (var kv in vectorParams)
            {
                shader.SetVector4(kv.Key, kv.Value);
            }

            foreach (var kv in textureParams)
            {
                shader.SetTexture(kv.Key, kv.Value);
            }

            GameBase.Instance.GraphicsDevice.RasterizerState = rasterizerState;
        }


        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //_asset?.Dispose();
                }
                //_asset = null;
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Material()
        {
            Dispose(false);
        }
        #endregion


    }

}
