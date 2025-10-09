using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class BoxCollider : Collider
    {
        [SerializeField] public BoundingBox _box = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        public override void OnEnable()
        {
            base.OnEnable();
            _handle = physMan.AddStatic(transform, _box);
        }

        public override void internal_ColliderDirty()
        {
            base.internal_Invalidate();

            if (_handle == null) return;
            physMan.UpdateStatic(_handle.Value, transform, _box);
        }

    }
}
