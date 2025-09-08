namespace MGAlienLib
{
    /// <summary>
    /// manager base class
    /// </summary>
    public abstract class ManagerBase
    {
        protected GameBase owner;

        public ManagerBase(GameBase owner)
        {
            this.owner = owner;
        }

        public virtual void OnPreUpdate() { }
        public virtual void OnPostUpdate() { }
        public virtual void OnPreDraw() { }
        public virtual void OnPostDraw() { }
        public virtual void OnPreInitialize() { }
        public virtual void OnPostInitialize() { }
        public virtual void OnPreLoadContent() { }
        public virtual void OnPostLoadContent() { }

    }
}
