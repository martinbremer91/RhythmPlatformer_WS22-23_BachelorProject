using UnityEngine;

namespace Gameplay
{
    public class CharacterInput : GameplayComponent
    {
        #region REFERENCES

        [SerializeField] private CharacterStateController stateController;

        #endregion

        public static bool GamepadDetected = true;

        public static InputState InputState;

        public override void OnUpdate()
        {
            if (GamepadDetected)
                DetectGamepadInput();
            else
                DetectKeyboardInput();
        }

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

        private void DetectGamepadInput()
        {
            InputState.DirectionalInput = Vector2.zero;

            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(xAxis) > .1f)
                InputState.DirectionalInput.x = xAxis;
            if (Mathf.Abs(yAxis) > .1f)
                InputState.DirectionalInput.y = yAxis;

            InputState.DashButton = GetButtonState("Fire1");
            InputState.JumpButton = GetButtonState("Fire2");

            ButtonState GetButtonState(string buttonString)
            {
                if (Input.GetButtonDown(buttonString))
                    return ButtonState.Down;
                if (Input.GetButton(buttonString))
                    return ButtonState.On;
                if (Input.GetButtonUp(buttonString))
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
