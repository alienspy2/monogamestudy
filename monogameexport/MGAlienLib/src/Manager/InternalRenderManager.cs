
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 툴과 디버깅을 위한 렌더링을 담당하는 클래스
    /// </summary>
    public class InternalRenderManager : ManagerBase
    {
        private Material BasicEffectMaterial;
        private List<Action<BasicEffect>> debugDrawCommands = new List<Action<BasicEffect>>();

        public InternalRenderManager(GameBase owner) : base(owner)
        {
        }
        public override void OnPostInitialize()
        {
            BasicEffectMaterial = new Material
            {
                renderPriority = 999,
                shader = owner.shaderManager.GetShaderByName("BasicEffect"),
            };
        }

        public void OnRenderQ(RenderState renderState)
        {
            var camera = renderState.camera;

            var basicEffect = (BasicEffect)BasicEffectMaterial.shader.effect;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = camera.matView;
            basicEffect.Projection = camera.matProjection;

            foreach (var command in debugDrawCommands)
            {
                command(basicEffect);
            }
        }

        public void OnEndRenderQ()
        {
            debugDrawCommands.Clear();
        }

        public void StackDrawCommand(Action<BasicEffect> command)
        {
            debugDrawCommands.Add(command);
        }

    }
}
