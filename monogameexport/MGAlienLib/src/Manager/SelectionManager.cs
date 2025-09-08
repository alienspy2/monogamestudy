
using System.Collections.Generic;

namespace MGAlienLib
{
    public class SelectionManager : ManagerBase
    {
        private List<GameObject> _selectedObjects = new List<GameObject>();

        public int count => _selectedObjects.Count;
        public GameObject[] gameObjects => _selectedObjects.ToArray();


        public SelectionManager(GameBase owner) : base(owner)
        {
        }

        public void AddToSelection(GameObject gameObject)
        {
            if (!_selectedObjects.Contains(gameObject))
            {
                _selectedObjects.Add(gameObject);
            }
        }

        public void RemoveFromSelection(GameObject gameObject)
        {
            if (_selectedObjects.Contains(gameObject))
            {
                _selectedObjects.Remove(gameObject);
            }
        }

        public bool Contains(GameObject gameObject)
        {
            return _selectedObjects.Contains(gameObject);
        }

        public void ClearSelection()
        {
            _selectedObjects.Clear();
        }

        public GameObject GetLastSelection()
        {
            if (_selectedObjects.Count > 0)
            {
                return _selectedObjects[_selectedObjects.Count - 1];
            }

            return null;
        }
    }
}
