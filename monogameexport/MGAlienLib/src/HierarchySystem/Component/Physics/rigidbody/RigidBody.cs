using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class RigidBody : ComponentBase
    {
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
    }
}
