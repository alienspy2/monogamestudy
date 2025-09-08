
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public abstract class MGObject
    {
        protected Guid _Id = GUIDGenerator.Generate();

        public Guid Id
        {
            get => _Id;
            private set
            {
                _Id = value;
            }
        }

        public virtual void GetReadyForSerialize() { }

        public virtual void FinalizeDeserialize(DeserializeContext context) { }

    }

    public abstract class MGDisposableObject : MGObject, IDisposable
    {

        public abstract void Dispose();

    }
}
