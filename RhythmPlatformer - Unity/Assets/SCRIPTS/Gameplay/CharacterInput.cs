using Scriptable_Object_Scripts;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterInput : GameplayComponent
    {
        #region REFERENCES

        [SerializeField] private CharacterStateController _characterStateController;
        public ControlSettings ControlSettings;

        #endregion

        #region VARIABLES

        private static DefaultControls _controls;
        public static DefaultControls s_Controls
        {
            get
            {
                if (_controls == null)
                    _controls = new DefaultControls();
                return _controls;
            }
        }
        
        public InputState InputState;

        #endregion

        private void Awake()
        {
            s_Controls.GameplayDefault.AnalogMove.performed += 
                ctx => HandleAnalogMove(ctx.ReadValue<Vector2>());
            s_Controls.GameplayDefault.AnalogMove.canceled += 
                _ => InputState.DirectionalInput = Vector2.zero;
            InputState.analogDeadzone = true;

            s_Controls.GameplayDefault.DigitalUp.performed += _ => HandleDigitalMove(Vector2.up);
            s_Controls.GameplayDefault.DigitalDown.performed += _ => HandleDigitalMove(Vector2.down);
            s_Controls.GameplayDefault.DigitalLeft.performed += _ => HandleDigitalMove(Vector2.left);
            s_Controls.GameplayDefault.DigitalRight.performed += _ => HandleDigitalMove(Vector2.right);
            
            s_Controls.GameplayDefault.DigitalUp.canceled += _ => HandleDigitalMove(Vector2.up, true);
            s_Controls.GameplayDefault.DigitalDown.canceled += _ => HandleDigitalMove(Vector2.down, true);
            s_Controls.GameplayDefault.DigitalLeft.canceled += _ => HandleDigitalMove(Vector2.left, true);
            s_Controls.GameplayDefault.DigitalRight.canceled += _ => HandleDigitalMove(Vector2.right, true);
            
            s_Controls.GameplayDefault.Jump.performed += _ => _characterStateController.JumpSquat = true;
            s_Controls.GameplayDefault.Dash.performed += _ => _characterStateController.DashWindup = true;
#if UNITY_EDITOR
            s_Controls.GameplayDefault.DebugToggle.performed += _ => ToggleDebugMode();
#endif
            s_Controls.Enable();
        }

        public override void OnFixedUpdate()
        {
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            
            InputStateButtonsUpdate();

            void InputStateButtonsUpdate()
            {
                InputState.DashButton = s_Controls.GameplayDefault.Dash.phase;
                InputState.JumpButton = s_Controls.GameplayDefault.Jump.phase;
                InputState.WallClingTrigger = s_Controls.GameplayDefault.WallCling.phase;

                InputState.directionalInputModifier = 
                    s_Controls.GameplayDefault.DigitalAxesModifier.phase == InputActionPhase.Performed &&
                    InputState.analogDeadzone;
            }
        }

        private void HandleAnalogMove(Vector2 in_input)
        {
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            
            InputState.analogDeadzone = in_input.magnitude <= ControlSettings.InputDeadZone;
            
            if (InputState.analogDeadzone)
            {
                InputState.DirectionalInput = Vector2.zero;
                return;
            }
            
            InputState.DirectionalInput = in_input;
        }

        private void HandleDigitalMove(Vector2 in_digitalInput, bool in_cancel = false)
        {
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            
            RecordInputState();
            SetDigitalDirection();

            void SetDigitalDirection()
            {
                if (in_digitalInput.y == 0)
                {
                    float xValue = 
                        !in_cancel ? in_digitalInput.x : InputState.DigitalLeft ? -1 : InputState.DigitalRight ? 1 : 0;

                    InputState.DirectionalInput = new Vector2(xValue, InputState.DirectionalInput.y);
                }
                else
                {
                    float yValue =
                        !in_cancel ? in_digitalInput.y : InputState.DigitalDown ? -1 : InputState.DigitalUp ? 1 : 0;
                    
                    InputState.DirectionalInput = new Vector2(InputState.DirectionalInput.x, yValue);
                }
            }
            
            void RecordInputState()
            {
                if (in_digitalInput == Vector2.up)
                    InputState.DigitalUp = !in_cancel;
                else if (in_digitalInput == Vector2.down)
                    InputState.DigitalDown = !in_cancel;
                else if (in_digitalInput == Vector2.left)
                    InputState.DigitalLeft = !in_cancel;
                else
                    InputState.DigitalRight = !in_cancel;
            }
        }
        
#if UNITY_EDITOR
        private void ToggleDebugMode()
        {
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            
            GameStateManager.s_DebugMode = !GameStateManager.s_DebugMode;
            Debug.Log("Debug Movement: " + GameStateManager.s_DebugMode);
        }
#endif
    }
}
