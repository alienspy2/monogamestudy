
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// Shader class
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// Effect instance
        /// </summary>
        public Effect effect { get; private set; }
        /// <summary>
        /// Name of the shader
        /// </summary>
        public string name { get; private set; }
        /// <summary>
        /// List of parameter names
        /// </summary>
        public List<string> ParameterNames = new();

        /// <summary>
        /// Create a new instance of Shader
        /// </summary>
        /// <param name="effect"></param>
        public Shader(string name, Effect effect)
        {
            this.name = name;
            this.effect = effect;
            foreach (var param in effect.Parameters)
            {
                ParameterNames.Add(param.Name);
            }
        }

        /// <summary>
        /// Set float value to the shader
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetFloat(string name, float value)
        {
            if (ParameterNames.Contains(name))
            {
                effect.Parameters[name]?.SetValue(value);
            }
            else
            {
                Logger.Log($"Shader {this.name} does not have float parameter {name}");
            }
        }

        /// <summary>
        /// Set texture value to the shader
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetTexture(string name, Texture2D value)
        {
            if (ParameterNames.Contains(name))
            {
                effect.Parameters[name].SetValue(value);
            }
            else
            {
                Logger.Log($"Shader {this.name} does not have texture parameter {name}");
            }
        }

        public void SetVector4(string key, Vector4 value)
        {
            if (ParameterNames.Contains(key))
            {
                effect.Parameters[key].SetValue(value);
            }
            else
            {
                Logger.Log($"Shader {this.name} does not have vector4 parameter {key}");
            }
        }

        public void SetVector4Array(string key, Vector4[] value)
        {
            if (ParameterNames.Contains(key))
            {
                effect.Parameters[key].SetValue(value);
            }
            else
            {
                Logger.Log($"Shader {this.name} does not have vector4 array parameter {key}");
            }
        }
    }

    /// <summary>
    /// Shader manager class
    /// shader 는 생성후, 삭제하지 않는다.
    /// </summary>
    public sealed class ShaderManager : ManagerBase
    {
        private AssetManager assetManager => owner.assetManager;

        private Dictionary<string, Shader> shaders = new ();

        public ShaderManager(GameBase owner) : base(owner)
        {
        }

        public override void OnPreInitialize()
        {
            shaders["BasicEffect"] = new Shader("BasicEffect",
                new BasicEffect(owner.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    Projection = Matrix.Identity,
                    View = Matrix.Identity,
                    World = Matrix.Identity
                }); 
        }

        /// <summary>
        /// Load shader from the path
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public Shader LoadShader(string name, string path)
        {
            return (shaders[name] = new Shader(name, assetManager.GetEffect(path)));
        }

        /// <summary>
        /// Get shader by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Shader GetShaderByName(string name)
        {
            return shaders[name];
        }
        
        /// <summary>
        /// Get effect by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Effect GetEffectByName(string name)
        {
            return shaders[name].effect;
        }
    }
}
