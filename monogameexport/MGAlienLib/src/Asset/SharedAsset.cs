using System;
using System.Collections.Generic;
using System.Text;

namespace MGAlienLib
{
    /// <summary>
    /// 공유 자원을 참조 카운팅 방식으로 관리하는 추상 클래스입니다.
    /// </summary>
    /// <typeparam name="T">관리할 자원의 타입으로, IDisposable을 구현해야 합니다.</typeparam>
    public abstract class SharedAsset<T> : AssetBase where T : class, IDisposable
    {
        /// <summary>
        /// 공유 자원을 관리하고 참조를 생성 및 해제하는 매니저 클래스입니다.
        /// </summary>
        protected class Manager
        {
            private Dictionary<string, SharedAsset<T>> sharedAssets = new();

            /// <summary>
            /// 지정된 소스와 주소를 통해 공유 자원을 가져옵니다. 이미 존재하면 기존 자원을 반환합니다.
            /// </summary>
            /// <param name="source">자원의 소스</param>
            /// <param name="address">자원의 고유 주소</param>
            /// <param name="parameters">자원 요처시, 생성이 필요한 경우 전달할 parameter</param>
            /// <param name="factory">자원을 생성하는 팩토리 함수</param>
            /// <returns>공유 자원에 대한 참조</returns>
            public Reference Get(eAssetSource source, string address, object parameters, Func<eAssetSource, string, object, SharedAsset<T>> factory)
            {
                var key = address;
                if (sharedAssets.ContainsKey(key) == false)
                {
                    sharedAssets.Add(key, factory(source, address, parameters));
                }
                return sharedAssets[key].internal_CreateReference();
            }

            /// <summary>
            /// 지정된 자원 참조를 해제합니다. 참조 카운트가 0이 되면 자원을 제거합니다.
            /// </summary>
            /// <param name="assetRef">해제할 자원 참조</param>
            public void Release(Reference assetRef)
            {
                if (assetRef == null || assetRef.isValid == false) return;

                var address = assetRef.address;
                if (assetRef.internal_Release() && sharedAssets.ContainsKey(address))
                {
                    sharedAssets.Remove(address);
                }
            }

            /// <summary>
            /// 관리 중인 모든 자원을 해제하고 컬렉션을 비웁니다.
            /// 실행중 이 함수를 호출하면 절대 안됩니다.
            /// 앱이 꺼지는 중에만 호출해야 합니다.
            /// </summary>
            public void Clear()
            {
                foreach (var asset in sharedAssets.Values)
                {
                    asset.Dispose();
                }
                sharedAssets.Clear();
            }

            /// <summary>
            /// 현재 관리 중인 자원의 상태를 문자열로 반환합니다.
            /// </summary>
            /// <returns>자원 목록과 참조 카운트를 포함한 문자열</returns>
            public override string ToString()
            {
                StringBuilder sb = new();
                sb.AppendLine($"{this.GetType()}:");
                foreach (var kv in sharedAssets)
                {
                    sb.AppendLine($"  {kv.Key}({kv.Value.internal_referenceCount})");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 공유 자원에 대한 참조를 나타내며, 자원의 유효성과 접근을 관리합니다.
        /// </summary>
        public class Reference
        {
            /// <summary>
            /// 참조가 유효한지 여부를 나타냅니다.
            /// </summary>
            public bool isValid => source != null;

            /// <summary>
            /// 내부 참조 카운트를 반환합니다.
            /// </summary>
            public int internal_ReferenceCount => source.internal_referenceCount;

            /// <summary>
            /// 자원의 주소를 반환합니다.
            /// </summary>
            public string address => source.address;

            /// <summary>
            /// 관리 중인 자원을 반환합니다.
            /// </summary>
            /// <exception cref="ObjectDisposedException">참조가 유효하지 않을 경우 발생</exception>
            public T asset
            {
                get
                {
                    if (source == null)
                    {
                        throw new ObjectDisposedException(nameof(Reference));
                    }
                    return source.asset;
                }
            }

            private SharedAsset<T> source;

            /// <summary>
            /// 새로운 자원 참조를 생성합니다.
            /// </summary>
            /// <param name="source">참조할 공유 자원</param>
            public Reference(SharedAsset<T> source)
            {
                this.source = source;
                source.AddReference();
            }

            /// <summary>
            /// 이 참조를 해제합니다.
            /// </summary>
            public void Release()
            {
                manager.Release(this);
            }

            // internal 메서드는 일반적으로 문서화하지 않음 (내부 사용 전용)
            public bool internal_Release()
            {
                var disposed = source.Release();
                source = null;
                return disposed;
            }

            /// <summary>
            /// 이 참조의 복사본을 생성합니다.
            /// </summary>
            /// <returns>새로운 참조 객체</returns>
            /// <exception cref="ObjectDisposedException">참조가 유효하지 않을 경우 발생</exception>
            public Reference Clone()
            {
                if (source == null)
                {
                    throw new ObjectDisposedException(nameof(Reference));
                }
                return source.internal_CreateReference();
            }
        }

        private static readonly Lazy<Manager> _manager = new Lazy<Manager>(() => new Manager());

        /// <summary>
        /// 공유 자원을 관리하는 싱글톤 매니저 인스턴스입니다.
        /// </summary>
        static protected Manager manager => _manager.Value;

        /// <summary>
        /// 현재 자원의 내부 참조 카운트를 가져옵니다.
        /// </summary>
        public int internal_referenceCount { get; private set; } = 0;

        private T _asset;

        /// <summary>
        /// 상속 클래스에서 접근 가능한 관리 중인 자원입니다.
        /// </summary>
        protected T asset => _asset;

        protected void internal_SetExternalAsset(T externalAsset)
        {
            _asset = externalAsset;
        }

        /// <summary>
        /// 기본 소스를 사용하여 공유 자원을 생성합니다.
        /// </summary>
        /// <param name="address">자원의 고유 주소</param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter </param>
        public SharedAsset(string address, object parameters) : this(GameBase.Instance.assetManager.defaultSource, address, parameters)
        {
        }

        /// <summary>
        /// 지정된 소스와 주소를 사용하여 공유 자원을 생성합니다.
        /// </summary>
        /// <param name="source">자원의 소스</param>
        /// <param name="address">자원의 고유 주소</param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter </param>
        public SharedAsset(eAssetSource source, string address, object parameters)
        {
            this.address = address;
            _asset = CreateAsset(source, address, parameters);
            internal_referenceCount = 0;
        }

        /// <summary>
        /// 자원을 생성하는 추상 메서드로, 상속 클래스에서 구현해야 합니다.
        /// </summary>
        /// <param name="source">자원의 소스</param>
        /// <param name="address">자원의 고유 주소</param>
        /// <param name="parameters">자원을 생성해야 할 때 전달되는 parameter </param>
        /// <returns>생성된 자원</returns>
        protected abstract T CreateAsset(eAssetSource source, string address, object parameters);

        public Reference internal_CreateReference()
        {
            return new Reference(this);
        }

        protected void AddReference()
        {
            internal_referenceCount++;
        }

        protected bool Release()
        {
            if (internal_referenceCount <= 0)
            {
                throw new InvalidOperationException("Reference count cannot be decreased below zero.");
            }
            internal_referenceCount--;
            if (internal_referenceCount <= 0)
            {
                Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 관리 중인 모든 자원에 대한 정보를 문자열로 반환합니다.
        /// </summary>
        public static string debug_ManagerStatus => manager.ToString();

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _asset?.Dispose();
                }
                _asset = null;
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SharedAsset()
        {
            Dispose(false);
        }
        #endregion
    }
}
