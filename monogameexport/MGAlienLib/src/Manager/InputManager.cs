using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MGAlienLib
{
    /// <summary>
    /// 이 클래스는 입력을 관리합니다.
    /// </summary>
    public class InputManager : ManagerBase
    {
        protected KeyboardState keyboardState;
        protected KeyboardState oldKeyboardState;
        protected MouseState mouseState;
        protected MouseState oldMouseState;

        // todo : SDL ime 로 변경
        private UIInputField _uiInputFocus;
        private UIInputField _lastUIInputFocus;

        private bool _inputIsConsumedByUI = false;
        public bool InputIsConsumedByUI
        {
            get => _inputIsConsumedByUI;
            set => _inputIsConsumedByUI = value;
        }

        public InputManager(GameBase owner) : base(owner)
        {
            owner.Window.TextInput += OnTextInput;
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (_uiInputFocus != null)
            {
                _uiInputFocus.internal_OnInput(e.Character, e.Key);
            }
            else if (e.Key == Keys.Enter)
            {
                if (_lastUIInputFocus != null)
                {
                    if (_lastUIInputFocus.gameObject.active == false) return;
                    if (_lastUIInputFocus.enabled == false) return;
                    TryGetFocus(_lastUIInputFocus);
                }
            }
        }

        public bool TryGetFocus(UIInputField input)
        {
            if (_uiInputFocus != null)
            {
                _uiInputFocus.internal_OnLoseFocus();
                _lastUIInputFocus = _uiInputFocus;
                _uiInputFocus = null;
            }

            if (input == null) return false;

            _uiInputFocus = input;
            _uiInputFocus.internal_OnGetFocus();

            return true;
        }

        public override void OnPreUpdate()
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            _inputIsConsumedByUI = false;
        }

        public override void OnPostDraw()
        {
            oldKeyboardState = keyboardState;
            oldMouseState = mouseState;
        }

        /// <summary>
        /// 키가 이번 프레임에 눌렸는지 확인합니다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool WasPressedThisFrame(Keys key)
        {
            return keyboardState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 키가 이번 프레임에 떼어졌는지 확인합니다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool WasReleasedThisFrame(Keys key)
        {
            return !keyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 키가 눌려있는지 확인합니다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 마우스 버튼이 이번 프레임에 눌렸는지 확인합니다.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool WasPressedThisFrame(eMouseButton button)
        {
            switch (button)
            {
                case eMouseButton.Left:
                    return mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;
                case eMouseButton.Right:
                    return mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released;
                case eMouseButton.Middle:
                    return mouseState.MiddleButton == ButtonState.Pressed && oldMouseState.MiddleButton == ButtonState.Released;
            }
            return false;
        }

        /// <summary>
        /// 마우스 버튼이 이번 프레임에 떼어졌는지 확인합니다.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool WasReleasedThisFrame(eMouseButton button)
        {
            switch (button)
            {
                case eMouseButton.Left:
                    return mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed;
                case eMouseButton.Right:
                    return mouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed;
                case eMouseButton.Middle:
                    return mouseState.MiddleButton == ButtonState.Released && oldMouseState.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }

        /// <summary>
        /// 마우스 버튼이 눌려있는지 확인합니다.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsPressed(eMouseButton button)
        {
            switch (button)
            {
                case eMouseButton.Left:
                    return mouseState.LeftButton == ButtonState.Pressed;
                case eMouseButton.Right:
                    return mouseState.RightButton == ButtonState.Pressed;
                case eMouseButton.Middle:
                    return mouseState.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }

        /// <summary>
        /// screen space 에서 마우스 위치를 반환합니다.
        /// screen space 는 왼쪽 상단이 (0,0) 이고, 오른쪽이 +X 아래쪽이 +Y 입니다.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetMousePos()
        {
            return new Vector2(mouseState.X, mouseState.Y);
        }

        /// <summary>
        /// screen space 에서 마우스 위치 변화량을 반환합니다. 오른쪽이 +X 아래쪽이 +Y 입니다.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetMousePosDelta()
        {
            return new Vector2(mouseState.X - oldMouseState.X, mouseState.Y - oldMouseState.Y);
        }

        /// <summary>
        /// /screen space 에서 마우스 휠 변화량을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public float GetMouseWheelDelta()
        {
            return mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;
        }

    }
}
