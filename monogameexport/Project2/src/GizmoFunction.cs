using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class GizmoFunction : ComponentBase
    {
        private GameObject _selectedObject;
        private Camera _targetCamera;

        public void Select(GameObject selectedObject)
        {
            _selectedObject = selectedObject;
        }

        public void Activate()
        {

        }

        public void Deactivate()
        {

        }

        public override void Update()
        {
            base.Update();
            if (_selectedObject != null)
            {
                
            }
        }
    }
}
