
namespace MGAlienLib
{
    public class SphereBody : RigidBody
    {
        [SerializeField] public float _radius = 0.5f;

        public float radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    internal_BodyDirty();
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            const float adjust = 1f / 1.25f; // 왜인지는 모르겠으나, 1.25f 로 나누어야함.
            _handle = physMan.AddBody(transform, _radius * adjust, _mass);
        }


        public override void internal_BodyDirty()
        {
            base.internal_BodyDirty();

            if (_handle == null) return;

            physMan.UpdateBody(_handle.Value, transform, _radius, _mass);
        }
    }
}
