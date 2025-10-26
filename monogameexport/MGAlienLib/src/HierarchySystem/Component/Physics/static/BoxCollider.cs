using Microsoft.Xna.Framework;

namespace MGAlienLib
{
    public class BoxCollider : Collider
    {
        [SerializeField] protected BoundingBox _box = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        public BoundingBox box
        {
            get { return _box; }
            set
            {
                if (_box != value)
                {
                    _box = value;
                    internal_ColliderDirty();
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            internal_UpdatePhysics();
        }

        public override void internal_ColliderDirty()
        {
            base.internal_ColliderDirty();
            internal_UpdatePhysics();
        }

        private void internal_UpdatePhysics()
        {
            _handle = physMan.AddOrUpdateStatic(_handle, transform, _box);
        }

    }
}
