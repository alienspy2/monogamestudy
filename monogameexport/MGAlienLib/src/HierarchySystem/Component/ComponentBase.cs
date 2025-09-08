
using System;
using System.Collections;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 컴포넌트의 기본 클래스입니다.
    /// todo : coroutine 추가
    /// </summary>
    public class ComponentBase : MGDisposableObject
    {
        [SerializeDTO] public Guid _gameObjectId;

        /// <summary>
        /// 컴포넌트가 속한 게임 오브젝트를 가져옵니다.
        /// </summary>
        public GameObject gameObject;

        private List<Coroutine> _activeCoroutines = new List<Coroutine>();
        private List<MicroRoutine> _activeMicroToutines = new List<MicroRoutine>();

        /// <summary>
        /// 화면 관련 정보를 제공하는 Utility class 입니다
        /// </summary>
        protected static class Screen
        {
            /// <summary>
            /// 화면의 가로 크기를 가져옵니다.
            /// </summary>
            public static int width => GameBase.Instance.backbufferWidth;
            /// <summary>
            /// 화면의 세로 크기를 가져옵니다.
            /// </summary>
            public static int height => GameBase.Instance.backbufferHeight;
            /// <summary>
            /// 이번 frame에 화면의 크기가 변경되었는지 여부를 가져옵니다.
            /// </summary>
            public static bool screenSizeWasChangedThisFrame => GameBase.Instance.screenSizeChangedThisFrame;
        }

        protected SelectionManager Selection => GameBase.Instance.selectionManager;

        /// <summary>
        /// 시간 관련 정보를 제공하는 Utility class 입니다
        /// </summary>
        protected static class Time
        {
            /// <summary>
            /// 게임이 시작된 이후의 시간을 가져옵니다.
            /// </summary>
            public static float time => GameBase.Instance.time;
            /// <summary>
            /// 지난 프레임에서 현재 프레임까지의 시간을 가져옵니다.
            /// </summary>
            public static float deltaTime => GameBase.Instance.deltaTime;

            /// <summary>
            /// 게임이 시작된 이후의 프레임 수를 가져옵니다.
            /// </summary>
            public static int frameCount => GameBase.Instance.frameCount;
        }

        /// <summary>
        /// 컴포넌트가 파괴되었는지 여부를 가져옵니다.
        /// </summary>
        public bool destroyed { get; private set; } = false;

        private bool _started = false;
        private bool? _lastEnabled = null;

        protected GameBase game => GameBase.Instance;
        protected HierarchyManager hierarchyManager => game.hierarchyManager;
        protected InputManager inputManager => game.inputManager;
        protected AssetManager assetManager => game.assetManager;
        protected ShaderManager shaderManager => game.shaderManager;
        protected FontManager fontManager => game.fontManager;
        protected DynamicTextureAtlasManager atlasMan => game.dynamicTextureAtlasManager;

        protected BuiltinUIManager uiman => game.builtinUIManager;
        protected SelectionManager selectionManager => game.selectionManager;

        protected UIDebugConsolePanel console => uiman.console;

        protected GameBase.DefaultAssets defaultAssets => game.defaultAssets;

        protected Transform root => hierarchyManager.GetRootTransform();

        /// <summary>
        /// 새로운 GameObject를 생성합니다.
        /// GameObject는 new GameObject() 로 생성할 수 없습니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(string name, Transform parent) => hierarchyManager.CreateGameObject(name, parent);


        /// <summary>
        /// 컴포넌트가 속한 게임 오브젝트의 트랜스폼을 가져옵니다.
        /// </summary>
        public Transform transform => gameObject.transform;

        /// <summary>
        /// 컴포넌트가 속한 게임 오브젝트의 이름을 가져옵니다.
        /// 만약 게임 오브젝트가 이미 파괴되었다면 "(null)"을 반환합니다.
        /// </summary>
        public string name
        {
            get
            {
                if (this == null) return "(null)";
                else if (gameObject == null) return "(null)";
                return gameObject.name;
            }
            set
            {
                if (this == null) return;
                else if (gameObject == null) return;
                gameObject.name = value;
            }
        }

        [SerializeField] protected bool _enabled = true;

        /// <summary>
        /// 컴포넌트가 활성화되어 있는지 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (_enabled)
                    {
                        if (gameObject.active) OnEnable();
                    }
                    else
                    {
                        if (gameObject.active) OnDisable();
                    }
                }
            }
        }

        /// <summary>
        /// Component 는 new 로 생성할 수 없습니다.
        /// AddComponent 를 사용하여 생성해주세요.
        /// </summary>
        public ComponentBase() { }

        /// <summary>
        /// 컴포넌트가 활성화될 때 호출됩니다.
        /// 컴포넌트가 속한 gameObject 가 꺼진 상태에서 생성되는 등
        /// Awake 가 호출 될 수 없는 상황일 때에는
        /// 호출 가능해지는 시점에 호출됩니다.
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// 컴포넌트가 시작될 때 호출됩니다.
        /// Awake 와 비슷하나, 모든 컴포넌트의 Awake 가 호출된 이후에 호출됩니다.
        /// Start 가 호출 될 수 없는 상황일 때에는
        /// 호출 가능해지는 시점에 호출됩니다.
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// *사용불가* 내부적으로 호출되는 함수입니다.
        /// </summary>
        public void internal_PreUpdate()
        {
            if (game.exiting) return;
            PreUpdate();

            if (game.exiting) return;

            for (int i = _activeCoroutines.Count - 1; i >= 0; i--)
            {
                if (!_activeCoroutines[i].Update(Time.deltaTime))
                {
                    _activeCoroutines.RemoveAt(i); // 완료된 코루틴 제거
                    if (game.exiting) return;
                }
            }

            for (int i = _activeMicroToutines.Count - 1; i >= 0; i--)
            {
                if (!_activeMicroToutines[i].Update(Time.deltaTime))
                {
                    _activeMicroToutines.RemoveAt(i); // 완료된 마이크로루틴 제거
                    if (game.exiting) return;
                }
            }

        }

        public virtual void PreUpdate()
        {

        }

        public void internal_Update()
        {
            if (game.exiting) return;

            if ((_enabled && _lastEnabled == null)
                || (_lastEnabled.HasValue && _lastEnabled.Value == false))
            {
                _lastEnabled = true;
                OnEnable();
            }

            if (game.exiting) return;

            if (!_enabled && (_lastEnabled.HasValue && _lastEnabled.Value == true))
            {
                _lastEnabled = false;
                OnDisable();
            }

            if (game.exiting) return;

            if (!_started)
            {
                _started = true;
                Start();
            }

            Update();
        }

        /// <summary>
        /// 컴포넌트가 업데이트될 때 마다 호출됩니다.
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Update 와 비슷하게 컴포넌트가 업데이트될 때 마다 호출됩니다.
        /// 단, 모든 Update 가 호출된 이후에 호출됩니다.
        /// </summary>
        public virtual void LateUpdate()
        {

        }

        /// <summary>
        /// 컴포넌트가 활성화될 때 호출됩니다.
        /// 컴포넌트가 속한 gameObject 가 꺼진 상태등
        /// 호출 되기 불가능 한 상황일 때에는
        /// 호출 가능해지는 시점에 호출됩니다.
        /// </summary>
        public virtual void OnEnable()
        {
        }

        /// <summary>
        /// 컴포넌트가 비활성화될 때 호출됩니다.
        /// </summary>
        public virtual void OnDisable()
        {
        }

        /// <summary>
        /// 컴포넌트가 파괴될 때 호출됩니다.
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 게임 오브젝트를 파괴합니다.
        /// 즉시 파괴되지 않고, 예약 되었다가, 다음 프레임 시작 전에 일괄 파괴됩니다.
        /// </summary>
        /// <param name="obj"></param>
        public void Destroy(GameObject obj)
        {
            obj.DestroyAtEndOfUpdate();
        }

        /// <summary>
        /// 컴포넌트를 파괴합니다.
        /// </summary>
        /// <param name="component"></param>
        public void Destroy(ComponentBase component)
        {
            gameObject.RemoveComponent(component);
        }

        /// <summary>
        /// *사용불가* 내부적으로 호출되는 함수입니다.
        /// </summary>
        public void internal_OnDestroy()
        {
            if (gameObject.active && enabled)
            {
                OnDisable();
            }

            OnDestroy();
            destroyed = true;
        }

        /// <summary>
        /// *사용불가* 내부적으로 호출되는 함수입니다.
        /// </summary>
        public void internal_CheckLifeCycle()
        {
            if (enabled)
            {
                if (!_lastEnabled.HasValue || _lastEnabled.Value)
                {
                    _lastEnabled = false;
                    OnDisable();
                }
            }
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            var cor = new Coroutine(routine);
            _activeCoroutines.Add(cor);
            return cor;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            if (_activeCoroutines.Contains(coroutine))
                _activeCoroutines.Remove(coroutine);
        }

        public MicroRoutine StartMicroRoutine(MicroRoutine.MicroRoutineDelegate routine, object data)
        {
            var cor = new MicroRoutine(routine, data);
            _activeMicroToutines.Add(cor);
            return cor;
        }

        public void StopMicroRoutine(MicroRoutine coroutine)
        {
            if (_activeMicroToutines.Contains(coroutine))
                _activeMicroToutines.Remove(coroutine);
        }

        public override string ToString()
        {
            return $"{name}";//:{(enabled?"O":"X")}:{GetType()}";
        }

        public static bool operator ==(ComponentBase a, object b)
        {
            if (a is null) return b is null;
            if (b is null) return a.destroyed;

            if (b is ComponentBase cb)
            {
                return a.Id == cb.Id;
            }

            return false;
        }

        public static bool operator !=(ComponentBase a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("ComponentBase".GetHashCode(), Id);
        }

        /// <summary>
        /// 컴포넌트를 추가합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : ComponentBase => gameObject.AddComponent<T>();

        /// <summary>
        /// 컴포넌트를 제거합니다.
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(ComponentBase component) => gameObject.RemoveComponent(component);


        /// <summary>
        /// 컴포넌트를 하나 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : ComponentBase => gameObject.GetComponent<T>();

        /// <summary>
        /// 컴포넌트를 *모두* 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponents<T>() where T : ComponentBase => gameObject.GetComponents<T>();

        /// <summary>
        /// 자식을 모두 찾아 컴포넌트를 하나 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInChildren<T>() where T : ComponentBase => gameObject.GetComponentInChildren<T>();

        /// <summary>
        /// 자식을 모두 찾아 컴포넌트를 *모두* 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponentsInChildren<T>() where T : ComponentBase => gameObject.GetComponentsInChildren<T>();

        public T GetComponentInParent<T>() where T : ComponentBase => gameObject.GetComponentInParent<T>();

        public override void GetReadyForSerialize()
        {
            base.GetReadyForSerialize();
            if (gameObject != null)
            {
                _gameObjectId = gameObject.Id;
            }
            else
            {
                _gameObjectId = Guid.Empty;
            }
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            if (_gameObjectId != Guid.Empty)
            {
                var owner = (GameObject)context.objectPool[_gameObjectId];
                owner.internal_LinkComponent(this);
            }
            else
            {
                throw new Exception("ComponentBase: gameObjectId is empty");
            }
        }

        public virtual void OnDispose()
        {

        }

        public virtual void internal_Invalidate()
        {

        }

        #region IDisposable Support
        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    OnDispose();
                }
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ComponentBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
