using Microsoft.Xna.Framework;

namespace MGAlienLib
{
    public class RigidBody : ComponentBase
    {
        [SerializeField] protected float _mass = 1f;

        public float mass
        {
            get { return _mass; }
            set
            {
                if (_mass != value)
                {
                    _mass = value;
                    internal_BodyDirty();
                }
            }
        }

        protected BepuPhysics.BodyHandle? _handle = null;

        protected Vector3 _velocity = Vector3.Zero;
        protected Quaternion _rotation = Quaternion.Identity;
        protected Vector3 _position = Vector3.Zero;

        public Vector3 velocity => _velocity;

        public override void OnDisable()
        {
            base.OnDisable();
            if (_handle.HasValue)
            {
                physMan.RemoveBody(_handle.Value);
                _handle = null;
            }
        }

        public void Resolve()
        {
            if (!_handle.HasValue) return;

            game.physicsManager.GetBodyReference(_handle.Value,
                            ref _position,
                            ref _rotation,
                            ref _velocity);
        }

        public override void Update()
        {
            base.Update();

            if (!_handle.HasValue) return;

            Resolve(); // todo : physics update 주기를 따로 가져간다면, 그 타이밍에 해야한다

            transform.position = _position;
            transform.rotation = _rotation;
        }

        public virtual void internal_BodyDirty()
        {

        }

    }
}
