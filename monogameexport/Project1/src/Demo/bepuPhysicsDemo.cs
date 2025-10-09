
using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Project1
{
    internal class bepuPhysicsDemo : _3DDemoBase
    {
        private class Ball
        {
            public bepuPhysicsDemo owner;
            public GameBase game => owner.game;

            public Transform ballTransform;
            public BepuPhysics.BodyHandle sphereHandle;

            public Ball(bepuPhysicsDemo owner, Vector3 position)
            {
                this.owner = owner;
                sphereHandle = game.physicsManager.test_addSphere(position, .5f);
                var ballObj = owner.hierarchyManager.CreateGameObject("ball", owner.transform).transform;
                var rdr = ballObj.AddComponent<MeshRenderer>();
                rdr.Load("raw://EditorResources/sphere.glb");
                rdr.LoadMaterial("MG/3D/Lit");
                ballTransform = ballObj.transform;
                ballTransform.localPosition = position;
                ballTransform.localScale = Vector3.One;

            }

            public void Update()
            {
                Vector3 vel = Vector3.Zero;
                Quaternion rot = Quaternion.Identity;
                Vector3 pos = Vector3.Zero;
                game.physicsManager.GetBodyReference(sphereHandle,
                                ref pos,
                                ref rot,
                                ref vel);
                ballTransform.position = pos;
                ballTransform.rotation = rot;
            }
        }

        List<Ball> balls = new();

        public override void Awake()
        {
            base.Awake();
            game.physicsManager.test_addStaticBox();
        }

        public override void Update()
        {
            base.Update();

            var random = new System.Random();

            if (inputManager.IsPressed(Keys.Space))
            {
                var pos = new Vector3(random.NextSingle() * 3f, 
                    10, 
                    random.NextSingle() * 3f);
                balls.Add(new Ball(this, pos));
            }

            foreach (var ball in balls)
            {
                ball.Update();
            }
        }
    }
}
