using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class SphereBody : RigidBody
    {
        [SerializeField] public float _radius = 0.5f;
        [SerializeField] public float _mass = 1f;

        public float radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    // todo : update physics body

                }
            }
        }

        public float mass
        {
            get { return _mass; }
            set
            {
                if (_mass != value)
                {
                    _mass = value;
                    // todo : update physics body
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            const float adjust = 1f / 1.25f; // 왜인지는 모르겠으나, 1.25f 로 나누어야함.
            _handle = physMan.AddBody(transform, _radius * adjust, _mass);
        }

    }
}
