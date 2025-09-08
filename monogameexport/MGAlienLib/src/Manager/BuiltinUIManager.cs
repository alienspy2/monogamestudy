
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class BuiltinUIManager : ManagerBase
    {
        public static float PopupElevation = 3000f;

        public static float ConsoleElevation = 1000f;
        public static float InspectorElevation = 999f;
        public static float HierarchyViewElevation = 998f;

        public static float DefaultElevationEnd = 900f;
        public static float DefaultElevationStart = 500f;

        private Transform _uiRoot;
        private Camera _cam;
        private UIDebugConsolePanel _console;

        private GameObject _activePopupMenu;

        public Camera cam => _cam;
        public Transform uiRoot => _uiRoot;
        public UIDebugConsolePanel console => _console;

        public BuiltinUIManager(GameBase owner) : base(owner)
        {
        }

        public void ShowConsole(bool show)
        {
            if (_console != null)
            {
                _console.gameObject.SetActive(show);
            }
        }

        public bool IsConsoleVisible()
        {
            return _console != null && _console.gameObject.active;
        }

        public override void OnPreLoadContent()
        {
            base.OnPostLoadContent();

            _uiRoot = owner.hierarchyManager.CreateGameObject("uiRoot").transform;
            _uiRoot.AddComponent<UICanvas>().mode = eCanvasType.Screen;

            var uicamObj = owner.hierarchyManager.CreateGameObject("uicam", _uiRoot);
            _cam = uicamObj.AddComponent<Camera>();
            _cam.useAsUI = true;
            _cam.uiRoot = _uiRoot;
            _cam.orthographic = true;
            _cam.orthographicSize = owner.backbufferHeight / 2;
            _cam.nearClipPlane = 1000;
            _cam.farClipPlane = 10000;
            _cam.transform.position = new Vector3(0, 0, 5000);
            _cam.transform.LookAt(Vector3.Zero, Vector3.Up);
            _cam.clearFlags = Camera.eCameraClearFlag.Depth;
            uicamObj.layer = LayerMask.NameToLayer("UI");
            _cam.cullingMask = LayerMask.GetMask("UI");
            _cam.renderPriority = 1000; // UI layer 는 마지막에 render

            // log panel
            _console = UIDebugConsolePanel.Build(_uiRoot);
            _console.transform.hideInHierarchy = true;
            Logger.Pipe += _console.Log;


            ShowConsole(false);
            //ShowHierarchyView(false);
            //ShowInspectorPanel(false);
        }

        public override void OnPreUpdate()
        {
            base.OnPreUpdate();

            if (_cam.viewport.Width != owner.backbufferWidth ||
                _cam.viewport.Height != owner.backbufferHeight)
            {
                int width = owner.backbufferWidth;
                int height = owner.backbufferHeight;

                cam.viewport = new Rectangle(0, 0, width, height);
                cam.orthographicSize = (float)height / 2f;
                cam.aspectRatio = (float)width / (float)height;
                cam.transform.position = new Vector3(width / 2f, height / 2f, 5000);
            }

            if (_activePopupMenu != null)
            {
                var uit = _activePopupMenu.GetComponent<UITransform>();

                // esc 키로 닫기
                if (owner.inputManager.WasPressedThisFrame(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    ClosePopupMenu();
                }

                // menu 밖 클릭시 닫기
                if (owner.inputManager.WasPressedThisFrame(eMouseButton.Left) ||
                    owner.inputManager.WasPressedThisFrame(eMouseButton.Right))
                {
                    if (uit != null)
                    {
                        var mousePos = owner.inputManager.GetMousePos();
                        mousePos.Y = owner.backbufferHeight - mousePos.Y;
                        if (uit.accumulatedRect.Contains(mousePos) == false)
                        {
                            ClosePopupMenu();
                        }
                    }

                }

                if (uit != null)
                {
                    var rect = uit.anchoredRect;
                    if (rect.X < 0) rect.X = 0;
                    if (-rect.Y < 0) rect.Y = 0; // todo : pivot, anchor 고려
                    if (rect.X + rect.Width > owner.backbufferWidth)
                    {
                        rect.X = owner.backbufferWidth - rect.Width;
                    }
                    if (-rect.Y + rect.Height > owner.backbufferHeight)
                    {
                        rect.Y = -(owner.backbufferHeight - rect.Height);
                    }
                    uit.anchoredRect = rect;
                }
            }

        }



        public void ShowPopupMenu(List<string> items, Vector2 anchoredPositionInScreen, Action<int> callback, bool useFilter = false)
        {
            ClosePopupMenu();

            int fs = 11;
            float h = 20;
            Color color = Color.White;
            Color textColor = Color.White;

            var panel = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(_uiRoot,
                "popupMenu",
                "art/UI/white.png", true, false,
                new Rectangle(0, 0, 100, 100), BuiltinUIManager.PopupElevation,
                Vector2.UnitY, Vector2.UnitY,
                color: color.Dimming(0.4f), layer: "UI"
                );
            panel.UITransform.position = anchoredPositionInScreen;

            var stacker = panel.AddComponent<UIVStacker>();
            stacker.SetMargins(5, 5, 5, 5, 5);
            stacker.autoSize = true;

            UIInputField filter = null;
            if (useFilter)
            {
                filter = UIInputField.Build(panel.transform,
                    "filter", "art/UI/white.png", true, false,
                    new RectangleF(0, 0, 100, h), 0.1f,
                    fontSize: fs,
                    textColor: Color.Red.Dimming(0.5f),
                    deactivatedColor: Color.White.Dimming(0.6f),
                    activatedColor: Color.White.Dimming(0.7f));
                filter.onValueChanged = (inputField) =>
                {
                    SetPopupMenuFilter(inputField.text);
                };
                filter.TryGetFocus();
                filter.textOffset = new Vector2(8, 3);
            }

            float maxWidth = 0;
            foreach (var item in items)
            {
                var button = UIButton.Build(panel.transform,
                    item,
                    "art/UI/white.png", true, false,
                    new RectangleF(0, 0, 100, h), .1f,
                    color: color.Dimming(0.3f),
                    text: item,
                    fontName: "notoKR", fontSize: fs, textColor: textColor,
                    onCommand: (_) =>
                    {
                        GameObject.Destroy(panel.gameObject);
                        callback?.Invoke(Array.IndexOf(items.ToArray(), item));
                    },
                    pivot: Vector2.UnitY, anchor: Vector2.UnitY);
                maxWidth = Mathf.Max(maxWidth, button.text.MeasureSize(item).X);
                button.text.SetOutline(false);
            }

            // 버튼 크기 조정
            foreach (var button in panel.GetComponentsInChildren<UIButton>())
            {
                button.UITransform.size = new Vector2(maxWidth, button.UITransform.size.Y);
            }
            if (filter != null) filter.UITransform.size = new Vector2(maxWidth, h);

            _activePopupMenu = panel.gameObject;
        }

        public void ShowColorPicker(Vector2 anchoredPositionInScreen, Color oldValue, Action<Color> callback)
        {
            ClosePopupMenu();

            int fontSize = 12;
            Color color = Color.White;
            Color textColor = Color.Black;

            var panel = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(_uiRoot,
                "popupMenu",
                "art/UI/white.png", true, false,
                new Rectangle(0, 0, 200, 30 + 5 + 5), BuiltinUIManager.PopupElevation,
                Vector2.UnitY, Vector2.UnitY,
                color: Color.White.Dimming(0.6f), layer: "UI"
                );
            panel.UITransform.position = anchoredPositionInScreen;

            // todo : color picker UI 로 변경
            var temp_if = UIInputField.Build(panel.transform,
                "Input Field", "art/UI/white.png", true, false,
                new RectangleF(5, -5, 200 - 5 - 5, 30), 0.3f,
                fontSize: fontSize,
                textColor: textColor,
                onEndEdit: (_if) =>
                {
                    var text = _if.text;
                    if (text.Length == 8)
                    {
                        Color color = Color.White;
                        try
                        {
                            color = new Color(
                                byte.Parse(text.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                                byte.Parse(text.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                                byte.Parse(text.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                                byte.Parse(text.Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                            callback?.Invoke(color);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e);
                        }
                    }
                    GameObject.Destroy(panel.gameObject);
                },
                activatedColor: Color.White,
                deactivatedColor: Color.Gray);
            temp_if.TryGetFocus();
            temp_if.onCancelEdit = (_) =>
            {
                GameObject.Destroy(panel.gameObject);
            };
            string oldValueText = $"{oldValue.R:X2}{oldValue.G:X2}{oldValue.B:X2}{oldValue.A:X2}";
            temp_if.text = oldValueText;

            _activePopupMenu = panel.gameObject;
        }

        public void ShowTextPicker(Vector2 anchoredPositionInScreen, string oldValue, Action<string> callback)
        {
            ClosePopupMenu();

            int fontSize = 12;
            Color color = Color.White;
            Color textColor = Color.Black;

            var panel = AutoAtlasSpriteRenderer.BuildAsUI<AutoAtlasSpriteRenderer>(_uiRoot,
                "popupMenu",
                "art/UI/white.png", true, false,
                new Rectangle(0, 0, 200, 30 + 5 + 5), BuiltinUIManager.PopupElevation,
                Vector2.UnitY, Vector2.UnitY,
                color: Color.White.Dimming(0.6f), layer: "UI"
                );
            panel.UITransform.position = anchoredPositionInScreen;

            var temp_if = UIInputField.Build(panel.transform,
                "Input Field", "art/UI/white.png", true, false,
                new RectangleF(5, -5, 200 - 5 - 5, 30), 0.3f,
                fontSize: fontSize,
                textColor: textColor,
                onEndEdit: (_if) =>
                {
                    var text = _if.text;
                    callback?.Invoke(text);
                    GameObject.Destroy(panel.gameObject);
                },
                activatedColor: Color.White,
                deactivatedColor: Color.Gray);
            temp_if.TryGetFocus();
            temp_if.onCancelEdit = (_) =>
            {
                GameObject.Destroy(panel.gameObject);
            };
            temp_if.text = oldValue;

            _activePopupMenu = panel.gameObject;
        }

        private void SetPopupMenuFilter(string newFilter)
        {
            if (_activePopupMenu == null) return;

            var buttons = _activePopupMenu.GetComponentsInChildren<UIButton>();
            foreach (var button in buttons)
            {
                if (button.text.text.IsMidMatch(newFilter) == false)
                {
                    button.gameObject.SetActive(false);
                }
                else
                {
                    button.gameObject.SetActive(true);
                }
            }
        }

        public override void OnPostUpdate()
        {
            base.OnPostUpdate();
        }

        public void ClosePopupMenu()
        {
            if (_activePopupMenu != null)
            {
                GameObject.Destroy(_activePopupMenu);
                _activePopupMenu = null;
            }
        }
    }
}
