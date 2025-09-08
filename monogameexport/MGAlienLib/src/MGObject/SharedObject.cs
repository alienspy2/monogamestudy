
//using System;

//namespace MGAlienLib
//{
//    public abstract class SharedObject<T> : MGDisposableObject where T : class, IDisposable
//    {

//        public class Reference
//        {
//            public bool disposed => source == null;

//            public T obj
//            {
//                get
//                {
//                    if (source == null)
//                    {
//                        return null;
//                    }

//                    return source.obj;
//                }
//            }
//            private SharedObject<T> source;

//            public Reference(SharedObject<T> source)
//            {
//                this.source = source;
//                source.AddReference();
//            }

//            public bool internal_Release()
//            {
//                var disposed = source.Release();
//                source = null;
//                return disposed;
//            }

//            public Reference Clone()
//            {
//                return source.internal_CreateReference();
//            }
//        }


//        protected int referenceCount { get; private set; } = 0;

//        private T _obj;
//        protected T obj => _obj;

//        protected abstract T Create();

//        public Reference internal_CreateReference()
//        {
//            return new Reference(this);
//        }

//        protected void AddReference()
//        {
//            referenceCount++;
//        }

//        protected bool Release()
//        {
//            referenceCount--;
//            if (referenceCount <= 0)
//            {
//                Dispose();
//                return true;
//            }

//            return false;
//        }

//        ~SharedObject()
//        {
//            Dispose();
//        }

//        public override void Dispose()
//        {
//            if (_obj != null)
//            {
//                _obj.Dispose();
//                _obj = null;
//            }
//        }
//    }
//}
