

namespace MGAlienLib
{
    public class SphereCollider : Collider
    {
        [SerializeField] protected float _radius = 0.5f;

        public float radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value)
                {
                    _radius = value;
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
            base.internal_Invalidate();
            internal_UpdatePhysics();
        }

        private void internal_UpdatePhysics()
        {
            const float adjust = 1f / 1.25f; // 왜인지는 모르겠으나, 1.25f 로 나누어야함.
            _handle = physMan.AddOrUpdateStatic(_handle, transform, _radius * adjust);
        }
    }
}
