
using MGAlienLib;
using Microsoft.Xna.Framework;
using System;

namespace Project1
{
    public class RigidbodyBox : ComponentBase
    {
        public override void Awake()
        {
            base.Awake();

            var rdr = AddComponent<MeshRenderer>();
            rdr.Load("raw://EditorResources/box.glb");
            rdr.LoadMaterial("MG/3D/Lit");

            var rb = AddComponent<BoxBody>();

            var random = new Random();
            transform.scale = Vector3.One * (random.NextSingle() * 1f + .5f);
        }

        public override void Update()
        {
            base.Update();

            if (transform.position.Y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }
}
