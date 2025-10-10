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
            _handle = physMan.AddStatic(transform, _box);
        }

        public override void internal_ColliderDirty()
        {
            base.internal_ColliderDirty();

            if (_handle == null) return;

            physMan.UpdateStatic(_handle.Value, transform, _box);
        }

    }
}
