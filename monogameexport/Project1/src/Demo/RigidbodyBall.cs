

using MGAlienLib;

namespace Project1
{
    public class RigidbodyBall : ComponentBase
    {
        public override void Awake()
        {
            base.Awake();

            var rdr = AddComponent<MeshRenderer>();
            rdr.Load("raw://EditorResources/sphere.glb");
            rdr.LoadMaterial("MG/3D/Lit");

            var rb = AddComponent<SphereBody>();
        }

        public override void Update()
        {
            base.Update();

            if (transform.position.Y < -2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
