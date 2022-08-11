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
        [SerializeField] private SettingsConfigs _settings;

        #endregion

        #region VARIABLES

        private DefaultControls _controls;
        public InputState InputState;

        #endregion

        private void Awake()
        {
            _controls = new DefaultControls();

            _controls.GameplayDefault.AnalogMove.performed += 
                ctx => HandleAnalogMove(ctx.ReadValue<Vector2>());
            _controls.GameplayDefault.AnalogMove.canceled += 
                ctx => InputState.DirectionalInput = Vector2.zero;
            InputState.analogDeadzone = true;

            _controls.GameplayDefault.DigitalMove.performed += 
                ctx => HandleDigitalMove(ctx.ReadValue<Vector2>());

            //temp
            _controls.GameplayDefault.Jump.performed +=
                ctx => _characterStateController.JumpSquat = true;
            _controls.GameplayDefault.Dash.performed +=
                ctx => _characterStateController.DashWindup = true;
            
#if UNITY_EDITOR
            _controls.GameplayDefault.DebugToggle.performed += ctx => ToggleDebugMode();
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
            InputState.analogDeadzone = in_input.magnitude <= _settings.InputDeadZone;
            if (InputState.analogDeadzone)
            {
                InputState.DirectionalInput = Vector2.zero;
                return;
            }
            
            InputState.DirectionalInput = in_input;
        }

        private void HandleDigitalMove(Vector2 in_digitalInput)
        {
            if (InputState.analogDeadzone)
                InputState.DirectionalInput = in_digitalInput;
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
