

using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 게임 오브젝트의 계층 구조를 관리하는 매니저 클래스입니다.
    /// </summary>
    public sealed class HierarchyManager : ManagerBase
    {
        private GameObject _root;
        private List<GameObject> _destroyedQueues = new();
        private Dictionary<Guid, GameObject> _allObjects = new();

        public HierarchyManager(GameBase owner) : base(owner)
        {
            
        }

        public void AddGameObject(GameObject obj)
        {
            _allObjects.Add(obj.Id, obj);
            owner.renderQueue.OnAddGameObject(obj);
        }

        public void RemoveGameObject(GameObject obj)
        {
            _allObjects.Remove(obj.Id, out GameObject _);
            owner.renderQueue.OnRemoveGameObject(obj);
        }

        public void Intenal_CreateRoot()
        {
            _root = new GameObject("*root*");
        }

        /// <summary>
        /// 루트 트랜스폼을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public Transform GetRootTransform()
        {
            return _root.transform;
        }

        /// <summary>
        /// 디버깅용
        /// 모든 게임 오브젝트의 정보를 text 로 반환합니다.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public string debug_Describe(Transform node = null, int depth = 0)
        {
            string result = "";

            if (node == null)
            {
                Logger.Log("**** Hierarchy Dump");
                node = _root.transform;
                result = $"+{_root.name}\n";
            }

            foreach(var child in node.GetChildren())
            {
                result += new string(' ', (depth+1)*2) + '+' + child.gameObject.ToString() + "\n";
                result += debug_Describe(child, depth + 1);
            }

            if (depth == 0)
            {
                Logger.Log(result);
            }

            return result;
        }

        /// <summary>
        /// 게임 오브젝트를 파괴합니다.
        /// </summary>
        /// <param name="obj"></param>
        public void AddToDestroyQueue(GameObject obj)
        {
            _destroyedQueues.Add(obj);
        }

        /// <summary>
        /// hierarchy 에 등록된 모든 GameObject 와 Component 들을 lifecycle 을 돌립니다.
        /// </summary>
        public override void OnPostUpdate()
        {
            // todo : 최적화   필요
            List<GameObject> copiedAllObjects = new List<GameObject>(_allObjects.Values);

            foreach (var obj in copiedAllObjects)
            {
                if (owner.exiting) return;
                obj.internal_PreUpdate();
            }

            foreach (var obj in copiedAllObjects)
            {
                if (owner.exiting) return;
                obj.internal_Update();
            }

            foreach (var obj in _destroyedQueues.ToArray())
            {
                var p = obj.transform.parent;

                List<Transform> collector = new();
                obj.transform.GetDescendants(ref collector);
                foreach (var cobj in collector)
                {
                    cobj.SetParent(null);
                    RemoveGameObject(cobj.gameObject);
                    cobj.gameObject.internal_OnDestroy();
                }
            }
            _destroyedQueues.Clear();

            foreach (var obj in _allObjects.Values)
            {
                if (owner.exiting) return;
                obj.internal_LateUpdate();
            }
        }

        /// <summary>
        /// 새로운 게임 오브젝트를 생성합니다.
        /// GameObject 는 반드시 이 함수를 통해 생성해야 합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="initialActive"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(string name, Transform parent = null, bool initialActive = true)
        {
            var obj = new GameObject(name, initialActive);

            if (parent != null)
            {
                obj.transform.SetParent(parent);
                obj.layer = parent.gameObject.layer;
            }
            else
            {
                obj.transform.SetParent(_root.transform);
            }

            AddGameObject(obj);

            return obj;
        }
    }
}
