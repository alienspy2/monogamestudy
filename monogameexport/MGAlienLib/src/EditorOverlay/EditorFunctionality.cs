
using Microsoft.Xna.Framework.Input;

namespace MGAlienLib
{
    public class EditorFunctionality : ComponentBase
    {
        private UIHierarchyViewPanel _hierarchyViewPanel;
        private UIInspectorPanel _uIInspectorPanel;

        public UIHierarchyViewPanel hierarchyViewPanel => _hierarchyViewPanel;
        public UIInspectorPanel uIInspectorPanel => uIInspectorPanel;

        private GameObject _mainMenuObj;

        private bool visible = true;

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

            //var path = System.IO.Path.Combine(assetManager.rawAssetsRootPath, "prefabs/MainMenu.prefab");
            //var toml = System.IO.File.ReadAllText(path);
            //_mainMenuObj = Serializer.DeserializePrefab(toml);
            //_mainMenuObj.transform.SetParent(uiman.uiRoot);

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
        }

    }
}
