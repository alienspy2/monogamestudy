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

        private CullMode _cullMode = CullMode.CullCounterClockwiseFace;
        private bool _zWrite = true;
        private bool _zTest = true;

        public CullMode cullMode
        {
            get => _cullMode;
            set => _cullMode = value;
        }
        public bool zWrite
        {
            get => _zWrite;
            set => _zWrite = value;
        }


        public bool zTest
        {
            get => _zTest;
            set => _zTest = value;
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

        public void SetColor(string name, Color red)
        {
            SetVector4(name, red.ToVector4());
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

            // todo : 다른 방법으로 했을 때 에러가 발생해서 일단 이렇게 했지만, 
            // 매 프레임 생성하면 안된다.
            var rasterzierState = new RasterizerState()
            {
                CullMode = _cullMode
            };
            
            var depthStencilState = new DepthStencilState()
            {
                DepthBufferWriteEnable = zWrite,
                DepthBufferEnable = zTest
            };

            GameBase.Instance.GraphicsDevice.RasterizerState = rasterzierState;
            GameBase.Instance.GraphicsDevice.DepthStencilState = depthStencilState;
        }

        public Material internal_Clone()
        {
            var newMaterial = new Material();
            newMaterial.renderPriority = renderPriority;
            newMaterial.shader = shader;
            newMaterial.floatParams = new(floatParams);
            newMaterial.vectorParams = new(vectorParams);
            newMaterial.textureParams = new(textureParams);
            newMaterial.cullMode = cullMode;

            return newMaterial;
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
