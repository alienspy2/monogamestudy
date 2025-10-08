
using Microsoft.Xna.Framework.Input;

namespace MGAlienLib
{
    public class EditorFunctionality : ComponentBase
    {
        private UIHierarchyViewPanel _hierarchyViewPanel;
        public UIHierarchyViewPanel hierarchyViewPanel => _hierarchyViewPanel;

        private UIInspectorPanel _uIInspectorPanel;
        public UIInspectorPanel uIInspectorPanel => uIInspectorPanel;

        private SceneViewControl _sceneViewControl;
        public SceneViewControl sceneViewControl => _sceneViewControl;

        private bool visible = true;
        private GameObject? oldSelectedObject = null;


        public void ShowHierarchyView(bool show)
        {
            if (_hierarchyViewPanel != null)
            {
                _hierarchyViewPanel.gameObject.SetActive(show);
            }
        }

        public bool IsHierarchyViewVisible()
        {
            return _hierarchyViewPanel != null && _hierarchyViewPanel.gameObject.active;
        }

        public void ShowInspectorPanel(bool show)
        {
            if (_uIInspectorPanel != null)
            {
                _uIInspectorPanel.gameObject.SetActive(show);
            }
        }

        public bool IsInspectorPanelVisible()
        {
            return _uIInspectorPanel != null && _uIInspectorPanel.gameObject.active;
        }

        public override void Awake()
        {
            base.Awake();


            _hierarchyViewPanel = UIHierarchyViewPanel.Build(uiman.uiRoot);

            // inspector panel
            _uIInspectorPanel = UIInspectorPanel.Build(uiman.uiRoot);

            _hierarchyViewPanel.onNodeClicked = (node) =>
            {
                var t = node.data as Transform;
                if (t != null)
                {
                    _uIInspectorPanel?.SetTarget(t.gameObject);
                }

                if (inputManager.IsPressed(Keys.LeftControl) == true)
                {
                    if (selectionManager.Contains(t.gameObject) == false)
                    {
                        selectionManager.AddToSelection(t.gameObject);
                    }
                    else
                    {
                        selectionManager.RemoveFromSelection(t.gameObject);
                    }
                }
                else
                {
                    selectionManager.ClearSelection();
                    if (t != null)
                    {
                        selectionManager.AddToSelection(t.gameObject);
                    }
                }
            };

            var sceneViewControlObj = CreateGameObject("sceneViewControl", transform);
            _sceneViewControl = sceneViewControlObj.AddComponent<SceneViewControl>();
            _sceneViewControl.Deactivate();

            ShowHierarchyView(true);
            ShowInspectorPanel(true);
        }

        public void Show()
        {
            ShowHierarchyView(true);
            ShowInspectorPanel(true);
            visible = true;
        }

        public void Hide()
        {
            ShowHierarchyView(false);
            ShowInspectorPanel(false);
            visible = false;
        }

        public override void Update()
        {
            base.Update();

            if (inputManager.WasPressedThisFrame(Keys.OemTilde))
            {
                uiman.ShowConsole(!uiman.IsConsoleVisible());
            }

            if (inputManager.WasPressedThisFrame(Keys.F1))
            {
                visible = !visible;
                ShowHierarchyView(visible);
                ShowInspectorPanel(visible);
            }

            if (selectionManager.count > 0)
            {
                var obj = selectionManager.gameObjects[^1];
                if (obj != oldSelectedObject)
                {
                    _uIInspectorPanel?.SetTarget(obj);
                    oldSelectedObject = selectionManager.gameObjects[^1];
                }
            }
            else
            {
                if (oldSelectedObject != null)
                {
                    _uIInspectorPanel?.SetTarget(null);
                    oldSelectedObject = null;
                }
            }

        }

    }
}
