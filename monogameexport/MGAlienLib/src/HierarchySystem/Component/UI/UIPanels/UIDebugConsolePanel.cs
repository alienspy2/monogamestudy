
using MGAlienLib.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace MGAlienLib
{
    public class UIDebugConsolePanel : UIPanel
    {
        private readonly string FontName = "notoKR";
        private readonly int FontSize = 12;
        private List<UITransform> _logLines;
        private int _lineHeight = 20;
        private UIInputField _inputField;
        private Dictionary<string, Action<string>> _commands;
        private List<string> _history = new List<string>();
        private int _maxHistory = 100;
        private int _historyIndex = 0;

        public void AfterBuild()
        {
            useTitleBar = true;
            useAsUI = true;
            useResizer = true;
            useCloseButton = true;

            var vStacker = contentRoot.AddComponent<UIVStacker>();
            vStacker.autoSize = false;
            vStacker.SetMargins(10, 0, 10, 0, 0);

            _titleBG.GetComponent<AutoAtlasSpriteRenderer>().useSlice = false;
            contentRoot.GetComponent<AutoAtlasSpriteRenderer>().useSlice = false;

            _inputField = UIInputField.Build(contentRoot.transform,
                "Input Field", "art/UI/white.png", true, false,
                new RectangleF(0, -30, 400, 30), 0.3f,
                fontSize: 16,
                activatedColor: Color.White,
                deactivatedColor: Color.Gray);

            _inputField.UITransform.expandWidthToParent = true;
            _inputField.UITransform.expandWidthToParentMargin = 20f;

            var masking = AddComponent<UIMasking>();
            masking.useAsUI = true;
            //masking.scissorsID = 0;

            _logLines = new List<UITransform>();
            ResizeLinesIfNeeded();

            _inputField.onEndEdit = (_) => { 
                Interprete(_.text); 
                _.text = ""; 
                _inputField.TryGetFocus();
            } ;

            RegisterCommand("clear", (_) => Clear());
            RegisterCommand("help", (_) => Log("*registered command : \n" + string.Join("\n", _commands.Keys)));
            RegisterCommand("history", (_) => Log("*history : \n" + string.Join("\n", _history)));

            _inputField.TryGetFocus();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _inputField?.TryGetFocus();
        }

        public void Clear()
        {
            foreach (var line in _logLines)
            {
                line.GetComponentInChildren<TextRenderer>().text = "";
            }
        }

        public void RegisterCommand(string command, Action<string> action)
        {
            if (_commands == null)
            {
                _commands = new Dictionary<string, Action<string>>();
            }
            if (_commands.ContainsKey(command))
            {
                _commands[command] = action;
            }
            else
            {
                _commands.Add(command, action);
            }
        }

        public void UnregisterCommand(string command)
        {
            if (_commands == null) return;
            if (_commands.ContainsKey(command))
            {
                _commands.Remove(command);
            }
        }

        private void Interprete(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var tokens = text.Split(' ');
            if (tokens.Length == 0) return;
            var command = tokens[0];
            if (_commands.ContainsKey(command))
            {
                var args = text.Substring(command.Length).Trim();
                _commands[command]?.Invoke(args);
                AddCommandToHistory(text);
            }
            else
            {
                Log($"Unknown command: {command}");
            }
        }

        private void AddCommandToHistory(string command)
        {
            if (_history.Count >= _maxHistory)
            {
                _history.RemoveAt(0);
            }
            // 중복된 명령어는 삭제후, 다시 추가
            _history.Remove(command);
            _history.Add(command);
            _historyIndex = _history.Count;
        }

        private void RecallHistory(int index)
        {
            if (_history.Count == 0) return;
            if (index < 0 || index >= _history.Count) return;
            _inputField.text = _history[index];
        }

        bool isFirstUpdate = true;
        public override void Update()
        {
            base.Update();
            if (isFirstUpdate){
                Logger.Log("console update");
                isFirstUpdate = false;
            }

            if (_inputField.focus)
            {
                if (inputManager.WasPressedThisFrame(Keys.Up))
                {
                    _historyIndex--;
                    if (_historyIndex < 0) _historyIndex = 0;
                    RecallHistory(_historyIndex);
                }
                else if (inputManager.WasPressedThisFrame(Keys.Down))
                {
                    _historyIndex++;
                    if (_historyIndex >= _history.Count) _historyIndex = _history.Count - 1;
                    RecallHistory(_historyIndex);
                }
            }
        }

        protected override void OnCloseButtonClicked()
        {
            base.OnCloseButtonClicked();
            gameObject.SetActive(false);
        }

        public void Log(string message)
        {
            if (_logLines == null || _logLines.Count == 0) return;

            if (message.Contains("\n"))
            {
                var lines = message.Split('\n');
                foreach (var line in lines)
                {
                    Log(line.Trim());
                }
                return;
            }

            {
                // 위 부터
                //var line = _logLines[_logLines.Count - 1];
                //_logLines.RemoveAt(_logLines.Count - 1);
                //_logLines.Insert(0, line);
                //line.transform.SetAsFirstSibling();

                // 아래 부터
                var line = _logLines[0];
                _logLines.RemoveAt(0);
                _logLines.Add(line);
                line.transform.SetAsLastSibling();

                var text = line.GetComponentInChildren<TextRenderer>();
                text.text = message;
            }
        }

        protected override void OnContentSizeChanged(Vector2 newSize)
        {
            base.OnContentSizeChanged(newSize);
            ResizeLinesIfNeeded();
        }

        private void ResizeLinesIfNeeded()
        {
            if (_logLines == null) return;

            var optimalLinesCount = (int)((contentRoot.anchoredRect.Height-30) / _lineHeight);

            while (_logLines.Count > optimalLinesCount)
            {
                int removeIndex = 0;
                var line = _logLines[removeIndex];
                Destroy(line.gameObject);
                _logLines.RemoveAt(removeIndex);
            }

            while (_logLines.Count < optimalLinesCount)
            {
                int insertIndex = 0;

                var spacer = UISpacer.Build<UISpacer>(contentRoot.transform,
                    new RectangleF(0, 0, contentRoot.anchoredRect.Width, _lineHeight), 0);

                var text = TextRenderer.BuildAsUI(spacer.transform,
                    name + "_text", FontName, FontSize,
                    "_", Color.Wheat,
                    new RectangleF(0, 0, contentRoot.anchoredRect.Width, _lineHeight),
                    0.1f,
                    eHAlign.Left, eVAlign.Bottom,
                    layer: LayerMask.LayerToName(gameObject.layer));

                spacer.transform.SetAsFirstSibling();
                _inputField.transform.SetAsFirstSibling();

                _logLines.Insert(insertIndex, spacer.UITransform);
            }
        }

        public static UIDebugConsolePanel Build(Transform parent)
        {
            Color contentBGColor = Color.White.Dimming(0.1f);

            var logPanel = UIPanel.Build<UIDebugConsolePanel>(parent, "Log Panel",
                new RectangleF(0, -700, 400, 400), BuiltinUIManager.ConsoleElevation,
                true, true, true, true, false,
                titleBarColor: Color.Black,
                titleTextColor: Color.White,
                contentColor: contentBGColor,
                layer: "UI");

            logPanel.AfterBuild();

            

            return logPanel;
        }
    }
}
