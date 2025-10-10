
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class UITree : AutoAtlasSpriteRenderer
    {
        public static readonly bool IsAddableFromInspector = true;

        public class Node
        {
            public int depth;
            public object data;
            public bool foldOut = false; // true: 펴진상태, false: 접힌상태
            public AutoAtlasSpriteRenderer nodeButton;
            public AutoAtlasSpriteRenderer foldOutButton;
            public AutoAtlasSpriteRenderer expandOutButton;
            public TextRenderer nameRdr;
            public List<Node> children = new List<Node>();
            public Node parent;
            public UITree owner;

            public Node Add(UITree owner, string name, object data)
            {
                var newNode = new Node()
                {
                    data = data,
                    depth = depth + 1,
                    parent = this,
                    owner = owner
                };
                newNode.MakeRenderer(name);
                children.Add(newNode);

                return newNode;
            }

            public void Remove(Node node)
            {
                children.Remove(node);
            }

            public void Rename(string newName)
            {
                nameRdr.text = newName;
            }

            public void MakeRenderer(string name)
            {
                foldOutButton = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(owner.transform,
                    "foldOutButton",
                    "raw://EditorResources/icons/foldOutButton.png", false, false,
                    new RectangleF(4, -4, 16, 16), 0.1f,
                    pivot: Vector2.UnitY, anchor: Vector2.UnitY);
                foldOutButton.OnUICommand += (_) => Expand();

                expandOutButton = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(owner.transform,
                    "expandOutButton",
                    "raw://EditorResources/icons/expandOutButton.png", false, false,
                    new RectangleF(4, -4, 16, 16), 0.1f,
                    pivot: Vector2.UnitY, anchor: Vector2.UnitY);
                expandOutButton.OnUICommand += (_) => Fold();

                Color buttonColor = owner.color;
                if (data != null)
                {
                    var go = (data as Transform).gameObject;
                    if (GameBase.Instance.selectionManager.Contains(go))
                    {
                        buttonColor = Color.Red.Dimming(.5f);
                    }
                }

                nodeButton = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(owner.transform,
                    "nodeButton",
                    "raw://art/UI/white.png", true, false,
                    new RectangleF(0, 0, 100, 24), 0.1f,
                    color: buttonColor,
                    pivot: Vector2.UnitY, anchor: Vector2.UnitY);
                nodeButton.OnUICommand += (_) => owner.onNodeClicked?.Invoke(this);
                nodeButton.OnUIRightClick += (_)=> owner.onNodeRightClicked?.Invoke(this);

                nameRdr = TextRenderer.BuildAsUI(nodeButton.transform,
                    "_",
                    "notoKR", 12,
                    name, Color.White,
                    new RectangleF(0, 0, 100, 24), 0.1f,
                    layer: "UI");

            }

            public void Expand()
            {
                foldOut = true;
                if (GameBase.Instance.inputManager.IsPressed(Keys.LeftAlt))
                {
                    foreach (var child in children)
                    {
                        child.Expand();
                    }
                }
            }

            public void Fold()
            {
                foldOut = false;
                if (GameBase.Instance.inputManager.IsPressed(Keys.LeftAlt))
                {
                    foreach (var child in children)
                    {
                        child.Fold();
                    }
                }
            }

            //private void OnClick()
            //{
            //    Logger.Log($"{nameRdr.text} clicked in tree");
            //}

            public void UpdateStatus(ref int maxWidth, ref int y)
            {
                float x = 24 * depth;

                if (children.Count == 0)
                {
                    foldOutButton.enabled = false;
                    expandOutButton.enabled = false;
                }
                else
                {
                    if (foldOut)
                    {
                        foldOutButton.enabled = false;
                        expandOutButton.UITransform.position = new Vector2(x, y);
                        expandOutButton.enabled = true;
                    }
                    else
                    {
                        foldOutButton.UITransform.position = new Vector2(x, y);
                        foldOutButton.enabled = true;
                        expandOutButton.enabled = false;
                    }
                }

                if (parent != null && parent.accumulatedFoldOut() == false)
                {
                    nodeButton.enabled = false;
                    foldOutButton.enabled = false;
                    expandOutButton.enabled = false;
                    nameRdr.enabled = false;
                }
                else
                {
                    nodeButton.enabled = true;
                    nodeButton.UITransform.position = new Vector2(x + 30, y);
                    nodeButton.UITransform.size = nameRdr.presentSize;
                    nameRdr.enabled = true;

                    var newWidth = (int)(x + 30 + nameRdr.presentSize.X);
                    maxWidth = maxWidth < newWidth ? newWidth : maxWidth;

                    y -= 24;
                }

                foreach (var child in children)
                {
                    child.UpdateStatus(ref maxWidth, ref y);
                }
            }

            public bool accumulatedFoldOut()
            {
                if (parent != null) return foldOut && parent.accumulatedFoldOut();
                return foldOut;
            }

            public void GetAllNodes(ref List<Node> nodes)
            {
                nodes.Add(this);
                foreach (var child in children)
                {
                    child.GetAllNodes(ref nodes);
                }
            }
        }

        private Node _root;

        public Node GetRoot() => _root;

        public Action<Node> onNodeClicked = null;
        public Action<Node> onNodeRightClicked = null;

        public override void PreUpdate()
        {
            base.PreUpdate();
            int w = 0;
            int y = 0;
            _root.UpdateStatus(ref w, ref y);
            UITransform.parent.size = UITransform.size = new Vector2(w, -y);
        }

        private void MakeRoot()
        {
            _root = new Node();
            _root.owner = this;
            _root.MakeRenderer("Root");
            _root.depth = 0;
            _root.foldOut = true;
        }

        public static UITree Build(Transform parent,
            string name,
            string textureAddress, bool dialate, bool useSlice,
            RectangleF anchoredRect, float elevation = 0.1f,
            Vector2? pivot = null, Vector2? anchor = null,
            Color? color = null,
            string layer = "UI")
        {
            color ??= Color.White;

            anchor = pivot = Vector2.UnitY;
            var tree = AutoAtlasSpriteRenderer.BuildAsUI<UITree>(parent,
                name,
                textureAddress, dialate, useSlice,
                anchoredRect, elevation = 0.1f,
                pivot, anchor,
                color,
                layer);

            tree.MakeRoot();

            return tree;
        }

        public void Reset()
        {
            foreach (var child in transform.Children)
            {
                Destroy(child.gameObject);
            }

            MakeRoot();
        }

        public List<Node> GetAllNodes()
        {
            List<Node> nodes = new List<Node>();
            _root.GetAllNodes(ref nodes);
            return nodes;
        }
    }
}
