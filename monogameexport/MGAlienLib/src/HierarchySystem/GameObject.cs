
using System;
using System.Collections;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 게임 오브젝트입니다.
    /// 독립적인 기능은 거의 없고,
    /// Component 를 담는 그릇의 역할을 합니다.
    /// new GameObject 로 생성하지 말고,
    /// CreateGameObject 함수를 사용하세요.
    /// </summary>
    public class GameObject : MGObject
    {
        [SerializeField] private int _layer = 0;
        [SerializeField] private bool _active = true;
        [SerializeField] private string _name;

        public bool _destroyed { get; private set; } = false;
        private Transform _transform;
        private List<ComponentBase> _components = new List<ComponentBase>();

        public int layer
        {
            get => _layer;
            set
            {
                _layer = value;
            }
        }

        public bool active
        {
            get
            {
                if (transform.parent == null) return _active;
                return _active && transform.parent.gameObject.active;
            }
            set
            {
                if (_active != value)
                {
                    _active = value;
                    if (_active)
                    {
                        OnActivated();
                    }
                    else
                    {
                        OnDeactivated();
                    }
                }
            }
        }


        public string name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }

        public Transform transform
        {
            get => _transform;
        }


        private void OnActivated()
        {
            foreach (var t in transform)
            {
                if (t.gameObject.active)
                {
                    t.gameObject.OnActivated();
                }
            }
        }

        private void OnDeactivated()
        {
            // Create a copy of the list to avoid modification during iteration
            var transformCopy = new List<Transform>(transform);
            foreach (var t in transformCopy)
            {
                t.gameObject.OnDeactivated();
            }

            // call OnDisable
            foreach (var component in _components)
            {
                component.internal_CheckLifeCycle();
            }
        }


        // internal use only
        // don't use this
        public GameObject() { }


        public GameObject(string name, bool initialActive = true)
        {
            this._name = name;
            this._active = initialActive;
            _transform = AddComponent<Transform>();
        }

        public void internal_OnDestroy()
        {
            //Logger.Log($"{name}({Id}) destroyed");
            var p = transform.parent;

            transform.SetParent(null);
            foreach (var component in _components)
            {
                component.internal_OnDestroy();
                component.Dispose();
            }
            _components.Clear();

            _destroyed = true;
        }

        /// <summary>
        /// 컴포넌트를 추가합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : ComponentBase
        {
            var component = System.Activator.CreateInstance<T>();
            component.gameObject = this;
            internal_LinkComponent(component);
            return component;
        }

        public ComponentBase AddComponent(Type type)
        {
            var component = (ComponentBase)System.Activator.CreateInstance(type);
            component.gameObject = this;
            internal_LinkComponent(component);
            return component;
        }

        /// <summary>
        /// 컴포넌트를 제거합니다.
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(ComponentBase component)
        {
            component.internal_OnDestroy();
            component.Dispose();
            component.gameObject = null;
            _components.Remove(component);
        }

        /// <summary>
        /// 컴포넌트를 하나만 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : ComponentBase
        {
            foreach (var component in _components)
            {
                if (component is T)
                {
                    return (T)component;
                }
            }
            return null;
        }

        /// <summary>
        /// 이 gameObject 의 컴포넌트중 해당 type 에 해당하는 요소를 *모두* 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponents<T>() where T : ComponentBase
        {
            var collector = new List<T>();
            foreach (var component in _components)
            {
                if (component is T) { collector.Add((T)component); }
            }
            return collector;
        }

        /// <summary>
        /// 자식을 모두 찾아 컴포넌트를 하나만 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInChildren<T>() where T : ComponentBase
        { 
            return internal_GetComponentInChildren<T>(); 
        }

        private T internal_GetComponentInChildren<T>() where T : ComponentBase
        {
            foreach (var component in _components)
            {
                if (component is T) { return (T)component; }
            }

            foreach (var child in transform)
            {
                var rv = child.gameObject.internal_GetComponentInChildren<T>();
                if (rv != null) return rv;
            }

            return default(T);
        }

        /// <summary>
        /// 자식을 모두 찾아 컴포넌트를 *모두* 가져옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponentsInChildren<T>() where T : ComponentBase
        {
            var collector = new List<T>();
            internal_GetComponentsInChildren(ref collector);
            return collector;
        }

        private void internal_GetComponentsInChildren<T>(ref List<T> collector) where T : ComponentBase
        {
            foreach (var component in _components)
            {
                if (component is T) { collector.Add((T)component); }
            }

            foreach (var child in transform)
            {
                child.gameObject.internal_GetComponentsInChildren<T>(ref collector);
            }
        }

        public T GetComponentInParent<T>() where T : ComponentBase
        {
            var node = transform;
            do
            {
                if (node.gameObject.GetComponent<T>() != null)
                {
                    return node.gameObject.GetComponent<T>();
                }
                node = node.parent;
            } while (node != null);

            return null;
        }



        public void internal_PreUpdate()
        {
            if (!active) return;

            foreach (var component in _components)
            {
                if (component.enabled)
                {
                    component.internal_PreUpdate();
                }
            }
        }

        public void internal_Update()
        {
            if (!active) return;

            foreach (var component in _components.ToArray())
            {
                if (component.enabled)
                {
                    component.internal_Update();
                }
            }
        }

        public void internal_LateUpdate()
        {
            if (!active) return;

            foreach (var component in _components)
            {
                component.LateUpdate();
            }
        }

        public override string ToString()
        {
            string result = $"{(active ? "O" : "X")}{name} pos={transform.position}";
            foreach (var component in _components)
            {
                result += $" {component} ";
            }
            return result;
        }

        public void SetActive(bool value)
        {
            active = value;
        }

        public void DestroyAtEndOfUpdate()
        {
            GameBase.Instance.hierarchyManager.AddToDestroyQueue(this);
        }

        public List<ComponentBase> internal_GetComponents()
        {
            return _components;
        }

        public void internal_LinkComponent(ComponentBase component)
        {
            component.gameObject = this;
            if (component is Transform)
            {
                _transform = (Transform)component;
            }
            else if (component is UITransform uit)
            {
                var old_uit = GetComponent<UITransform>();
                if (old_uit != null)
                {
                    RemoveComponent(old_uit);
                }
            }
                
            _components.Add(component);
            component.Awake();
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            GameBase.Instance.hierarchyManager.AddGameObject(this);
        }

        public static bool operator ==(GameObject a, object b)
        {
            if (a is null) return b is null;
            if (b is null) return a._destroyed;

            if (b is GameObject cb)
            {
                return a.Id == cb.Id;
            }

            return false;
        }

        public static bool operator !=(GameObject a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("GameObject".GetHashCode(), Id);
        }

        internal static void Destroy(ComponentBase component)
        {
            component.gameObject.RemoveComponent(component);
        }

        internal static void Destroy(GameObject gameObject)
        {
            gameObject.DestroyAtEndOfUpdate();
        }

        internal void RemoveComponentOfTypeName(string typeName)
        {
            foreach(var component in _components.ToArray())
            {
                if (component.GetType().FullName == typeName)
                {
                    RemoveComponent(component);
                    break;
                }
            }
        }
    }
}
