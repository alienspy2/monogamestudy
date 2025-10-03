
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// draft : 씬의 계층 구조를 트리 뷰로 보여주는 패널을 나타냅니다.
    /// </summary>
    public class UIHierarchyViewPanel : UIPanel
    {
        private const string defaultImage = "raw://art/UI/white.png";

        private enum eMenuItems
        {
            None,
            AddNewObject,
            DeleteThis,
            CopyObject,
            PasteObject,
            SaveAsPrefab,
            InstanciatePrefab,
            Duplicate
        }

        private readonly List<(eMenuItems key,string text)> items = new List<(eMenuItems,string)>() {
                    (eMenuItems.AddNewObject, "Add new object"),
                    (eMenuItems.DeleteThis, "Delete this"),
                    (eMenuItems.CopyObject, "Copy object"),
                    (eMenuItems.PasteObject, "Paste object"),
                    (eMenuItems.SaveAsPrefab, "Save as prefab"),
                    (eMenuItems.InstanciatePrefab, "Instanciate prefab") ,
                    (eMenuItems.Duplicate, "Duplicate")
                };

        private readonly string FontName = "notoKR";
        private readonly int FontSize = 11;
        private UITree _sceneTreeView;

        public Action<UITree.Node> onNodeClicked
        {
            get => _sceneTreeView.onNodeClicked;
            set => _sceneTreeView.onNodeClicked = value;
        }

        private Dictionary<Guid, bool> _expandedCache = new Dictionary<Guid, bool>();

        private string clipboard = string.Empty;

        public void BuildSub()
        {
            Color contentBGColor = Color.White.Dimming(0.1f);

            useTitleBar = true;

            var refreshButton = UIButton.Build(_titleBG.transform,
                "Refresh Button",
                "raw://art/UI/refresh.png", false, false,
                new RectangleF(0, 0, 24, 24), 0.1f,
                onCommand: (_) => Refresh(),
                pivot: Vector2.UnitY, anchor: Vector2.UnitY);

            transform.hideInHierarchy = true;
            titleText?.SetOutline(true);

            UIScrollView scrollView = UIScrollView.Build(contentRoot.transform,
                "scrollview",
                new Rectangle(0, 0, 0, 10), 0.1f,
                true, true);

            _sceneTreeView = UITree.Build(scrollView.content.transform,
                "treeview",
                defaultImage, true, false,
                new Rectangle(0, 0, 0, 10),
                color: contentBGColor);
            _sceneTreeView.UITransform.expandWidthToParent = true;
            _sceneTreeView.UITransform.expandHeightToParent = true;

            _sceneTreeView.onNodeRightClicked = (node =>
            {
                var selectedTransform = node.data as Transform;
                var pos = inputManager.GetMousePos();
                pos.Y = -pos.Y;

                var itemsText = new List<string>();
                foreach(var item in items)
                {
                    itemsText.Add(item.text);
                }

                uiman.ShowPopupMenu(itemsText, pos, (index) =>
                {
                    if (items[index].key == eMenuItems.AddNewObject)
                    {
                        // Add component logic
                        var newObject = hierarchyManager.CreateGameObject("New Object", selectedTransform);
                    }
                    else if (items[index].key == eMenuItems.DeleteThis)
                    {
                        // Remove component logic
                        if (selectedTransform != null)
                        {
                            Destroy(selectedTransform.gameObject);
                        }
                    }
                    else if (items[index].key == eMenuItems.CopyObject)
                    {
                        var selected = Selection.GetLastSelection();
                        if (selectedTransform != null)
                        {
                            clipboard = Serializer.SerializePrefab(selected);
                        }
                    }
                    else if (items[index].key == eMenuItems.PasteObject)
                    {
                        var selected = Selection.GetLastSelection();
                        if (!clipboard.IsNullOrEmpty() && selected != null)
                        {
                            var newObject = Serializer.DeserializePrefab(clipboard);
                            newObject.transform.SetParent(selected.transform);
                            Refresh();
                        }
                    }
                    else if (items[index].key == eMenuItems.SaveAsPrefab)
                    {
                        if (selectedTransform != null)
                        {
                            var node = selectedTransform.gameObject;
                            var path = GtkUtility.ShowSaveFileDialog("Save prefab", GameBase.Instance.config.rawAssetsRootPath);

                            if (!path.IsNullOrEmpty())
                            {
                                var toml = Serializer.SerializePrefab(node);
                                try
                                {
                                    System.IO.File.WriteAllText(path, toml);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log(e);
                                }
                            }
                        }
                    }
                    else if (items[index].key == eMenuItems.InstanciatePrefab)
                    {
                        if (selectedTransform != null)
                        {
                            var node = selectedTransform.gameObject;
                            var path = GtkUtility.ShowOpenFileDialog("Select prefab", GameBase.Instance.config.rawAssetsRootPath);

                            if (!path.IsNullOrEmpty())
                            {
                                try
                                {
                                    var toml = System.IO.File.ReadAllText(path);
                                    var newObject = Serializer.DeserializePrefab(toml);
                                    newObject.transform.SetParent(selectedTransform.transform);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log(e.Message);
                                }
                            }
                        }
                    }
                    else if (items[index].key == eMenuItems.Duplicate)
                    {
                        var selected = Selection.GetLastSelection();
                        if (selected != null)
                        {
                            var toml = Serializer.SerializePrefab(selected);
                            var newObject = Serializer.DeserializePrefab(toml);
                            newObject.transform.SetParent(selected.transform.parent);
                            Refresh();
                        }
                    }
                });
            });
        }

        private void UpdateHierarchyViewRecursive(UITree.Node node, Transform transform)
        {
            node.data = transform;
            node.Rename(transform.gameObject.name);

            foreach (var t in transform.Children)
            {
                //Logger.Log($"adding {t.gameObject.name} : hide? {t.hideInHierarchy}");
                if (t.hideInHierarchy)
                    continue;
                var newNode = node.Add(_sceneTreeView, t.gameObject.name, t);
                UpdateHierarchyViewRecursive(newNode, t);
            }
        }

        public void Refresh()
        {
            // store the expanded state of each node
            {
                _expandedCache.Clear();
                var allNodes = _sceneTreeView.GetAllNodes();
                foreach (var node in allNodes)
                {
                    if (node.data == null) continue;
                    if (node.foldOut)
                    {
                        _expandedCache[((Transform)node.data).Id] = node.foldOut;
                    }
                }
            }

            _sceneTreeView.Reset();
            UpdateHierarchyViewRecursive(_sceneTreeView.GetRoot(), hierarchyManager.GetRootTransform());

            // restore the expanded state of each node
            {
                var allNodes = _sceneTreeView.GetAllNodes();
                foreach (var node in allNodes)
                {
                    var t = (Transform)node.data;
                    if (_expandedCache.ContainsKey(t.Id))
                    {
                        node.foldOut = _expandedCache[t.Id];
                    }
                }
            }
        }

        protected override void OnCloseButtonClicked()
        {
            base.OnCloseButtonClicked();
            gameObject.SetActive(false);
        }

        public static UIHierarchyViewPanel Build(Transform parent)
        {
            Color titleColor = new Color(89, 150, 134, 255);
            Color contentBGColor = Color.White.Dimming(0.1f);

            var treeviewPanel = UIPanel.Build<UIHierarchyViewPanel>(parent,
                "Hierarchy view Panel",
                new RectangleF(400, 0, 500, 600), BuiltinUIManager.HierarchyViewElevation,
                contentBGTexAddress: defaultImage,
                useTitleBar: true,
                useResizer: true,
                useVStacker: false,
                useCloseButton: true,
                useContentBgSlice: false,
                useContextSizeFitter: false,
                titleBarBGTexAddress: defaultImage,
                titleTextColor: Color.White,
                titleBarColor: titleColor,
                contentColor: contentBGColor);

            treeviewPanel.BuildSub();

            return treeviewPanel;
        }

    }
}