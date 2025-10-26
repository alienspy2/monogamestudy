
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

            var random = new Random();

            var rdr = AddComponent<MeshRenderer>();
            rdr.Load("raw://EditorResources/box.glb");
            rdr.LoadMaterial("MG/3D/Lit");
            rdr.BreakMaterialSharing();
            var randomColor = new Color(
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                1f);
            rdr.material.asset.SetColor("_BaseColor", randomColor);

            var rb = AddComponent<BoxBody>();

            transform.scale = Vector3.One * (random.NextSingle() * .4f + 1f);
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
