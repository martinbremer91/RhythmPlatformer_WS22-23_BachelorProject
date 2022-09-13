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

        #endregion

        #region VARIABLES

        private ControlSettings _controlSettings;
        public ControlSettings ControlSettings => _controlSettings;

        private DefaultControls _controls;
        public InputState InputState;

        #endregion

        private void Awake()
        {
            _controls = new DefaultControls();
            _controlSettings = GameplayReferenceManager.s_Instance.ControlSettings;

            _controls.GameplayDefault.AnalogMove.performed += 
                ctx => HandleAnalogMove(ctx.ReadValue<Vector2>());
            _controls.GameplayDefault.AnalogMove.canceled += 
                _ => InputState.DirectionalInput = Vector2.zero;
            InputState.analogDeadzone = true;

            _controls.GameplayDefault.DigitalUp.performed += _ => HandleDigitalMove(Vector2.up);
            _controls.GameplayDefault.DigitalDown.performed += _ => HandleDigitalMove(Vector2.down);
            _controls.GameplayDefault.DigitalLeft.performed += _ => HandleDigitalMove(Vector2.left);
            _controls.GameplayDefault.DigitalRight.performed += _ => HandleDigitalMove(Vector2.right);
            
            _controls.GameplayDefault.DigitalUp.canceled += _ => HandleDigitalMove(Vector2.up, true);
            _controls.GameplayDefault.DigitalDown.canceled += _ => HandleDigitalMove(Vector2.down, true);
            _controls.GameplayDefault.DigitalLeft.canceled += _ => HandleDigitalMove(Vector2.left, true);
            _controls.GameplayDefault.DigitalRight.canceled += _ => HandleDigitalMove(Vector2.right, true);
            
            _controls.GameplayDefault.Jump.performed += _ => _characterStateController.JumpSquat = true;
            _controls.GameplayDefault.Dash.performed += _ => _characterStateController.DashWindup = true;
#if UNITY_EDITOR
            _controls.GameplayDefault.DebugToggle.performed += _ => ToggleDebugMode();
#endif
            _controls.Enable();
        }

        private void Update()
        {
            InputStateButtonsUpdate();

            void InputStateButtonsUpdate()
            {
                InputState.DashButton = _controls.GameplayDefault.Dash.phase;
                InputState.JumpButton = _controls.GameplayDefault.Jump.phase;
                InputState.WallClingTrigger = _controls.GameplayDefault.WallCling.phase;

                InputState.directionalInputModifier = 
                    _controls.GameplayDefault.DigitalAxesModifier.phase == InputActionPhase.Performed &&
                    InputState.analogDeadzone;
            }
        }

        private void HandleAnalogMove(Vector2 in_input)
        {
            InputState.analogDeadzone = in_input.magnitude <= _controlSettings.InputDeadZone;
            
            if (InputState.analogDeadzone)
            {
                InputState.DirectionalInput = Vector2.zero;
                return;
            }
            
            InputState.DirectionalInput = in_input;
        }

        private void HandleDigitalMove(Vector2 in_digitalInput, bool in_cancel = false)
        {
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
            GameStateManager.s_DebugMode = !GameStateManager.s_DebugMode;
            Debug.Log("Debug Movement: " + GameStateManager.s_DebugMode);
        }
#endif
    }
}
