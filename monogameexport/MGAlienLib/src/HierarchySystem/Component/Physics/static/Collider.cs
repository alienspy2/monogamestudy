

namespace MGAlienLib
{
    public class Collider : ComponentBase
    {
        public const int invalidHandle = -1;
        protected BepuPhysics.StaticHandle? _handle = null;

        public override void OnDisable()
        {
            base.OnDisable();
            if (_handle.HasValue)
            {
                physMan.RemoveStatic(_handle.Value);
                _handle = null;
            }
        }

        public virtual void internal_ColliderDirty()
        {

        }

        protected void internal_Invalidate()
        {
            if (_handle.HasValue)
            {
                physMan.RemoveStatic(_handle.Value);
                _handle = null;
            }
            _handle = null;
            //_handle = internal_CreateCollider();
        }
    }
}
