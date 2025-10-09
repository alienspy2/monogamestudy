using MGAlienLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    internal class StaticBall : ComponentBase
    {
        public override void Start()
        {
            base.Start();
            var rdr = AddComponent<MeshRenderer>();
            rdr.Load("raw://EditorResources/sphere.glb");
            rdr.LoadMaterial("MG/3D/Lit");
            rdr.BreakMaterialSharing();
            rdr.material.asset.SetVector4("_BaseColor", new Microsoft.Xna.Framework.Vector4(.5f, 0, 0, 1));
            var rb = AddComponent<SphereCollider>();
        }
    }
}
