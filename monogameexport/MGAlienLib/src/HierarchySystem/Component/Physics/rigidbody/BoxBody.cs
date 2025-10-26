
using Microsoft.Xna.Framework;

namespace MGAlienLib
{
    public class BoxBody : RigidBody
    {
        [SerializeField] protected BoundingBox _box = new BoundingBox(Vector3.One * -0.5f, Vector3.One * .5f);

        public BoundingBox box
        {
            get { return _box; }
            set
            {
                if (_box != value)
                {
                    _box = value;
                    internal_BodyDirty();
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _handle = physMan.AddOrUpdateBody(_handle, transform, _box, _mass);
        }


        public override void internal_BodyDirty()
        {
            base.internal_BodyDirty();

            if (_handle == null) return;

            physMan.AddOrUpdateBody(_handle.Value, transform, _box, _mass);
        }

    }
}
