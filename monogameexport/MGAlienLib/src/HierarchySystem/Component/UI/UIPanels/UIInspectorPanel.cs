using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MGAlienLib
{
    public class UIInspectorPanel : UIPanel
    {
        public static readonly float DrawerHeight = 20;
        public static readonly int DrawerFontSize = 10;
        public static readonly int DrawerLineSpacing = 3;

        public GameObject target;
        private UIScrollView scrollView;

        private Dictionary<string, bool> _typeFoldOut = new Dictionary<string, bool>();

        public void AfterBuild()
        {
            useTitleBar = true;
            useResizer = true;

            var refreshButton = UIButton.Build(_titleBG.transform,
                "Refresh Button",
                "art/UI/refresh.png", false, false,
                new RectangleF(0, 0, 24, 24), 0.1f,
                onCommand: (_) => Refresh(),
                pivot: Vector2.UnitY, anchor: Vector2.UnitY);

            titleText.text = "Inspector";
            UITransform.size = new Vector2(400, 600);

            scrollView = UIScrollView.Build(contentRoot.transform,
                "scrollview",
                new RectangleF(0, 0, 100, 100), 0.1f,
                true, true);

            scrollView.UITransform.expandWidthToParent = true;
            scrollView.UITransform.expandHeightToParent = true;
            scrollView.content.AddComponent<UIVStacker>().SetMargins(10, 10, 10, 10, 3);

            transform.hideInHierarchy = true;
        }

        Vector2 oldSize = Vector2.Zero;
        public override void Update()
        {
            base.Update();
            if (target != null && oldSize != contentRoot.size && _resizeHold == false)
            {
                oldSize = contentRoot.size;
                Refresh();
            }
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
            if (target == null)
                return;

            titleText.text = $"Inspector: {target.name}";

            foreach (var child in scrollView.content.transform)
            {
                Destroy(child.gameObject);
            }

            int fs = DrawerFontSize;
            float w = UITransform.size.X - 30;
            float h = DrawerHeight;

            Action<string> AddTitle = (_text) =>
            {
                var spacer = UISpacer.Build<UISpacer>(
                    scrollView.content.transform,
                    new RectangleF(0, 0, w, h), 0.1f);

                var title = UIButton.Build(
                    spacer.transform, "title",
                    "art/UI/white.png", true, false,
                    new RectangleF(0, 0, w - h - 5, h), 0.2f,
                    text: _text, fontName: "notoKR", fontSize: fs, textColor: Color.White,
                    color: Color.Yellow.Dimming(0.4f));

                title.OnUICommand = (_) =>
                {
                    if (_typeFoldOut.ContainsKey(_text) == false)
                        _typeFoldOut.Add(_text, true);
                    else
                        _typeFoldOut[_text] = !_typeFoldOut[_text];
                    SetTarget(target);
                };

                var deleteButton = UIButton.Build(
                    spacer.transform, "delete",
                    "art/UI/white.png", true, false,
                    new RectangleF(w - h, 0, h, h), 0.2f,
                    text: "X", fontName: "notoKR", fontSize: fs, textColor: Color.White,
                    color: Color.Red.Dimming(0.6f));
                deleteButton.OnUICommand = (_) =>
                {
                    if (target != null)
                    {
                        // todo : 이렇게 하면, 같은 종류가 한번에 모두 지워진다.
                        // 선택한 component 만 지우도록 수정 필요
                        target.RemoveComponentOfTypeName(_text);
                        Refresh();
                    }
                };
            };

            // gameObject 의 properties
            SetTargetSub(target);

            foreach (var component in target.internal_GetComponents())
            {
                var typeName = component.GetType().FullName;
                AddTitle(typeName);
                bool foldOut = false;
                if (_typeFoldOut.ContainsKey(typeName))
                    foldOut = _typeFoldOut[typeName];

                if (foldOut == false)
                    SetTargetSub(component);
            }

            // add component button
            var addComponentButton = UIButton.Build(scrollView.content.transform, "addComponent",
                "art/UI/white.png", true, false,
                new RectangleF(0, 0, w, h), 0.1f,
                text: "Add Component", fontName: "notoKR", fontSize: fs, textColor: Color.White,
                color: Color.Green.Dimming(0.5f));

            addComponentButton.OnUICommand = (_) =>
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ComponentBase)))
                    .ToList();

                var addableTypes = new List<Type>();

                foreach(var type in allTypes)
                {
                    if (type.Name == "UIButton")
                    {
                        Logger.Log("!");
                    }
                    // type 내의 static bool IsAsable 변수의 값 확인
                    var addableField = type.GetField("IsAddableFromInspector", BindingFlags.Public | BindingFlags.Static);
                    var addableValue = addableField?.GetValue(null);
                    if (addableValue == null)
                        continue;
                    var addable = (bool)addableValue;
                    if (addable)
                        addableTypes.Add(type);
                }


                var typeNames = addableTypes.Select(t => t.Name).ToList();
                var pos = inputManager.GetMousePos();
                pos.Y = -pos.Y;
                uiman.ShowPopupMenu(typeNames, pos, (index) =>
                {
                    if (index >= 0 && index < typeNames.Count)
                    {
                        var selectedType = addableTypes[index];
                        var newComponent = target.AddComponent(selectedType);
                        Refresh();
                    }
                }, true);
            };

            oldSize = contentRoot.size;
        }

        private void Refresh()
        {
            if (target == null) return;
            SetTarget(target);
        }

        private void SetTargetSub(object target)
        {
            // 모든 어셈블리에서 찾기
            var asemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(target.GetType().FullName))
                .FirstOrDefault(t => t != null);

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // SerializeDTOAttribute 가 있는 애들은 패스
                var dtoAttributes = field.GetCustomAttributes(typeof(SerializeDTOAttribute), true);
                if (dtoAttributes.Length > 0)
                    continue;

                // SerializeFieldAttribute 가 있는 애들만
                var attributes = field.GetCustomAttributes(typeof(SerializeFieldAttribute), true);
                if (attributes.Length == 0)
                    continue;

                // SerializeFieldAttribute 얻기
                bool skip = false;
                foreach(var attr in attributes)
                {
                    if (attr is SerializeFieldAttribute serializeFieldAttribute)
                    {
                        if (serializeFieldAttribute.HideInInspector)
                            skip = true;
                    }
                }
                if (skip) continue;

                AddLine(field, target);
            }
        }

        private void AddLine(FieldInfo _f, object _o)
        {
            int fs = DrawerFontSize;
            float w = UITransform.size.X - 30;
            float h = DrawerHeight;

            string _n = _f.Name;
            string _t = _f.FieldType.Name;

            var spacer = UISpacer.Build<UISpacer>(
                scrollView.content.transform,
                new RectangleF(0, 0, w, h), 0.1f);

            // field name
            var fieldNameSpacer = UISpacer.Build<UISpacer>(
                spacer.transform,
                new RectangleF(0, 0, w / 2, h), 0.1f);

            TextRenderer.BuildAsUI(fieldNameSpacer.transform,
                "text", "notoKR", fs, _n, Color.White,
                new RectangleF(0, 0, w / 2, h), 0.1f);

            // value
            var fieldValueSpacer = UISpacer.Build<UISpacer>(
                spacer.transform,
                new RectangleF(w / 2, 0, w / 2, h), 0.1f);


            if (_t == typeof(int).Name)
            {
                MakeSingleValueDrawer<int>(fieldValueSpacer.transform, _f, _o);
            }
            //_t 가 List 라면
            else if (_f.FieldType.IsGenericType &&
                _f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList list = (IList)_f.GetValue(_o);
                spacer.UITransform.size = new Vector2(w, (list.Count + 1) * (h + DrawerLineSpacing));
                MakeList(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(float).Name)
            {
                MakeSingleValueDrawer<float>(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(bool).Name)
            {
                MakeToggleButton(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(string).Name)
            {
                bool browseFile = false;
                var attr = _f.GetCustomAttribute(typeof(SerializeFieldAttribute));
                if (attr != null)
                {
                    var serializeFieldAttribute = (SerializeFieldAttribute)attr;
                    browseFile = serializeFieldAttribute.BrowseFile;
                }

                if (browseFile)
                    MakeStringWithBrowseButton(fieldValueSpacer.transform, _f, _o);
                else
                    MakeSingleValueDrawer<string>(fieldValueSpacer.transform, _f, _o);
            }    
            else if (_t == typeof(Vector2).Name)
            {
                MakeVector2(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(Vector3).Name)
            {
                MakeVector3(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(Vector4).Name)
            {
                MakeVector4(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(Quaternion).Name)
            {
                MakeQuaternion(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(Color).Name)
            {
                MakeColor(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(Rectangle).Name)
            {
                MakeRectangle(fieldValueSpacer.transform, _f, _o);
            }
            else if (_t == typeof(RectangleF).Name)
            {
                MakeRectangleF(fieldValueSpacer.transform, _f, _o);
            }
            else if (_f.FieldType.IsGenericType &&
                _f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                MakeList(fieldValueSpacer.transform, _f, _o);
            }
            else if (_f.FieldType.IsEnum)
            {
                var method = this.GetType().GetMethod("MakeEnum", BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method.MakeGenericMethod(_f.FieldType);
                genericMethod.Invoke(this, new object[] { fieldValueSpacer.transform, _f, _o });
            }
            else
            {
                string _v;
                if (_f == null || _o == null)
                    _v = "(null)";
                else
                {
                    object? value = _f.GetValue(_o);
                    if (value == null)
                        _v = "(null)(2)";
                    else
                        _v = value.ToString();
                }
                TextRenderer.BuildAsUI(fieldValueSpacer.transform,
                    "text", "notoKR", fs, _v, Color.White,
                    new RectangleF(0, 0, w / 2, h), 0.1f);
            }

        }

        private void MakeStringWithBrowseButton(Transform parent, FieldInfo _f, object _o)
        {
            string _v = (string)_f.GetValue(_o);
            PropertyDrawer.MakeStringWithBrowseButton(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeSingleValueDrawer<T>(Transform parent, FieldInfo _f, object _o)
        {
            T _v = (T)_f.GetValue(_o);
            PropertyDrawer.MakeSingle<T>(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeToggleButton(Transform parent, FieldInfo _f, object _o)
        {
            bool _v = (bool)_f.GetValue(_o);
            PropertyDrawer.MakeToggleButton(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeVector2(Transform parent, FieldInfo _f, object _o)
        {
            Vector2 _v = (Vector2)_f.GetValue(_o);
            PropertyDrawer.MakeVector2(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeVector3(Transform parent, FieldInfo _f, object _o)
        {
            Vector3 _v = (Vector3)_f.GetValue(_o);
            PropertyDrawer.MakeVector3(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });

        }

        private void MakeVector4(Transform parent, FieldInfo _f, object _o)
        {
            Vector4 _v = (Vector4)_f.GetValue(_o);
            PropertyDrawer.MakeVector4(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeQuaternion(Transform parent, FieldInfo _f, object _o)
        {
            Quaternion _q = (Quaternion)_f.GetValue(_o);
            var _v = _q.ToEulerAngles();
            PropertyDrawer.MakeVector3(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        var _newQ = _newValue.FromEulerAnglesToQuaternion();
                        _f.SetValue(_o, _newQ);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeColor(Transform parent, FieldInfo _f, object _o)
        {
            Color _v = (Color)_f.GetValue(_o);
            PropertyDrawer.MakeColorButton(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeRectangle(Transform parent, FieldInfo _f, object _o)
        {
            Rectangle _v = (Rectangle)_f.GetValue(_o);
            PropertyDrawer.MakeRectangle(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeRectangleF(Transform parent, FieldInfo _f, object _o)
        {
            RectangleF _v = (RectangleF)_f.GetValue(_o);
            PropertyDrawer.MakeRectangleF(parent,
                _v, (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                    }
                });
        }

        private void MakeEnum<T>(Transform parent, FieldInfo _f, object _o) where T : Enum
        {
            // Get the enum values
            // Create a dropdown or similar UI element to select the enum value
            PropertyDrawer.MakeEnum<T>(parent, (T) _f.GetValue(_o), 
                (_newValue, _success) =>
                {
                    if (_success)
                    {
                        _f.SetValue(_o, _newValue);
                        if (_o is ComponentBase _co) _co.internal_Invalidate();
                        SetTarget(target);
                }
            });
        }

        private void MakeList(Transform parent, FieldInfo _f, object _o)
        {
            // Get the List object
            IList list = (IList)_f.GetValue(_o);
            if (list == null)
            {
                // Create a new instance of the list if it's null
                Type listType = _f.FieldType;
                list = (IList)Activator.CreateInstance(listType);
                _f.SetValue(_o, list);
            }

            // Get list element type
            Type elementType = _f.FieldType.GetGenericArguments()[0];

            // Create a vertical layout to contain list elements
            var listContainer = UISpacer.Build<UISpacer>(
                parent,
                new RectangleF(0, 0, parent.gameObject.GetComponent<UISpacer>().UITransform.size.X, DrawerHeight), 0.1f);

            var listLayout = listContainer.gameObject.AddComponent<UIVStacker>();
            listLayout.SetMargins(0, 0, 0, 3, 3);

            // Show list size
            var countRow = UISpacer.Build<UISpacer>(
                listContainer.transform,
                new RectangleF(0, 0, parent.gameObject.GetComponent<UISpacer>().UITransform.size.X, DrawerHeight), 0.1f);

            TextRenderer.BuildAsUI(countRow.transform,
                "size", "notoKR", DrawerFontSize, $"Size: {list.Count}", Color.White,
                new RectangleF(0, 0, countRow.UITransform.size.X * 0.7f, DrawerHeight), 0.1f);

            // Add button
            var addButton = UIButton.Build(
                countRow.transform, "add",
                "art/UI/white.png", true, false,
                new RectangleF(countRow.UITransform.size.X * 0.7f, 0, countRow.UITransform.size.X * 0.3f, DrawerHeight), 0.1f,
                color: Color.Green.Dimming(0.4f));

            TextRenderer.BuildAsUI(addButton.transform,
                "text", "notoKR", DrawerFontSize, "Add", Color.White,
                new RectangleF(0, 0, addButton.UITransform.size.X, DrawerHeight), 0.1f);

            addButton.OnUICommand = (_) => {
                // Create default value for the type
                object newItem = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
                list.Add(newItem);

                if (_o is ComponentBase _co) _co.internal_Invalidate();
                SetTarget(target);
            };

            // Display each element in the list
            for (int i = 0; i < list.Count; i++)
            {
                int index = i; // Capture the index for use in lambda
                var itemRow = UISpacer.Build<UISpacer>(
                    listContainer.transform,
                    new RectangleF(0, 0, parent.gameObject.GetComponent<UISpacer>().UITransform.size.X, DrawerHeight), 0.1f);

                var indexLabel = UISpacer.Build<UISpacer>(
                    itemRow.transform,
                    new RectangleF(0, 0, itemRow.UITransform.size.X * 0.1f, DrawerHeight), 0.1f);

                TextRenderer.BuildAsUI(indexLabel.transform,
                    "idx", "notoKR", DrawerFontSize, $"{index}:", Color.White,
                    new RectangleF(0, 0, indexLabel.UITransform.size.X, DrawerHeight), 0.1f);

                // Value container
                var valueContainer = UISpacer.Build<UISpacer>(
                    itemRow.transform,
                    new RectangleF(itemRow.UITransform.size.X * 0.1f, 0, itemRow.UITransform.size.X * 0.7f, DrawerHeight), 0.1f);

                // Create appropriate editor based on element type
                DisplayListItemValue(valueContainer.transform, elementType, list, index, _o);

                // Remove button
                var removeButton = UIButton.Build(
                    itemRow.transform, "remove",
                    "art/UI/white.png", true, false,
                    new RectangleF(itemRow.UITransform.size.X * 0.8f, 0, itemRow.UITransform.size.X * 0.2f, DrawerHeight), 0.1f,
                    color: Color.Red.Dimming(0.4f));

                TextRenderer.BuildAsUI(removeButton.transform,
                    "text", "notoKR", DrawerFontSize, "X", Color.White,
                    new RectangleF(0, 0, removeButton.UITransform.size.X, DrawerHeight), 0.1f);

                removeButton.OnUICommand = (_) => {
                    list.RemoveAt(index);
                    if (_o is ComponentBase _co) _co.internal_Invalidate();
                    SetTarget(target);
                };
            }
        }

        private void DisplayListItemValue(Transform parent, Type elementType, IList list, int index, object _o)
        {
            // Get the current value
            object value = list[index];

            if (elementType == typeof(int))
            {
                PropertyDrawer.MakeSingle<int>(parent,
                    (int)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(float))
            {
                PropertyDrawer.MakeSingle<float>(parent,
                    (float)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(bool))
            {
                // Assuming you have a MakeToggleButton implementation
                PropertyDrawer.MakeToggleButton(parent,
                    (bool)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(string))
            {
                PropertyDrawer.MakeSingle<string>(parent,
                    (string)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(Vector2))
            {
                PropertyDrawer.MakeVector2(parent,
                    (Vector2)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(Vector3))
            {
                PropertyDrawer.MakeVector3(parent,
                    (Vector3)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else if (elementType == typeof(Color))
            {
                PropertyDrawer.MakeColorButton(parent,
                    (Color)value, (_newValue, _success) => {
                        if (_success)
                        {
                            list[index] = _newValue;
                            if (_o is ComponentBase _co) _co.internal_Invalidate();
                            SetTarget(target);
                        }
                    });
            }
            else
            {
                // For unsupported types, just show the string representation
                TextRenderer.BuildAsUI(parent,
                    "text", "notoKR", DrawerFontSize, value?.ToString() ?? "(null)", Color.White,
                    new RectangleF(0, 0, parent.gameObject.GetComponent<UISpacer>().UITransform.size.X, DrawerHeight), 0.1f);
            }
        }

        protected override void OnCloseButtonClicked()
        {
            base.OnCloseButtonClicked();
            gameObject.SetActive(false);
        }

        public static UIInspectorPanel Build(Transform parent)
        {
            Color titleColor = new Color(89, 150, 134, 255);
            Color contentBGColor = Color.White.Dimming(0.1f);

            var inspector = UIPanel.Build<UIInspectorPanel>(parent,
              "Inspector",
              new RectangleF(0, 0, 400, 400), BuiltinUIManager.InspectorElevation,
              contentBGTexAddress: "art/UI/white.png",
              useTitleBar: true,
              useResizer: true,
              useVStacker: false,
              useCloseButton: true,
              useContentBgSlice: true,
              useContextSizeFitter: false,
              titleBarBGTexAddress: "art/UI/white.png",
              titleTextColor: Color.White,
              titleBarColor: new Color(100, 120, 140, 255),
              contentColor: Color.White.Dimming(0.1f));

            inspector.AfterBuild();


            return inspector;
        }
    }

}
