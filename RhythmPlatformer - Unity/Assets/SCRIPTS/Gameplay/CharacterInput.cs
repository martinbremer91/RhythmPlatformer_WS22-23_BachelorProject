using UnityEngine;

namespace Gameplay
{
    public class CharacterInput : GameplayComponent
    {
        #region REFERENCES

        [SerializeField] private CharacterStateController stateController;

        #endregion

        public static InputState InputState;

        public override void OnUpdate() => DetectKeyboardInput();

        private void DetectKeyboardInput()
        {
            InputState.DirectionalInput = Vector2.zero;
            
            if (Input.GetKey(KeyCode.A))
                InputState.DirectionalInput.x -= 1;
            if (Input.GetKey(KeyCode.D))
                InputState.DirectionalInput.x += 1;

            if (Input.GetKey(KeyCode.W))
                InputState.DirectionalInput.y += 1;
            if (Input.GetKey(KeyCode.S))
                InputState.DirectionalInput.y -= 1;

            InputState.DashButton = GetButtonState(KeyCode.Space);
            InputState.JumpButton = GetButtonState(KeyCode.J);

            ButtonState GetButtonState(KeyCode key)
            {
                if (Input.GetKeyDown(key))
                    return ButtonState.Down;
                if (Input.GetKey(key))
                    return ButtonState.On;
                if (Input.GetKeyUp(key))
                    return ButtonState.Up;
                
                return ButtonState.Off;
            }
        }
    }

    public struct InputState
    {
        public Vector2 DirectionalInput;

        public ButtonState DashButton;
        public ButtonState JumpButton;
    }

    public enum ButtonState
    {
        Off = 0,
        Down = 1,
        On = 2,
        Up = 3
    }
}
