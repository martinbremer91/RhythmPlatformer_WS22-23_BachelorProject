using GlobalSystems;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using Structs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterInput : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES

        private GameStateManager _gameStateManager;

        public GameplayControlConfigs GameplayControlConfigs;
        private CharacterStateController _characterStateController;

        #endregion
        
        #region VARIABLES

        public UpdateType UpdateType => UpdateType.GamePlay;

        private DefaultControls _controls;
        
        public InputState InputState;

#if UNITY_EDITOR
        public static bool s_DebuggerStart;
#endif

    #endregion

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;

            GameplayControlConfigs = in_gameStateManager.GameplayControlConfigs;
            _characterStateController = in_gameStateManager.CharacterStateController;
            
            _controls = new DefaultControls();
            
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
            
            _controls.GameplayDefault.Dash.performed += _ => HandleDashButton();
#if UNITY_EDITOR
            _controls.DebugInputs.DebugJump.performed += _ => InputState.JumpCommand = true;
            _controls.DebugInputs.DebugToggle.performed += _ => ToggleDebugMode();

            _controls.DebugInputs.DebuggerStart.performed += _ => { s_DebuggerStart = true; };
            _controls.DebugInputs.DebuggerStart.canceled += _ => { s_DebuggerStart = false; };
#endif
            _controls.Enable();
        }

        public void CustomUpdate()
        {          
            InputStateButtonsUpdate();

            void InputStateButtonsUpdate()
            {
                if (_gameStateManager.ActiveUpdateType is not UpdateType.GamePlay)
                    return;

#if UNITY_EDITOR
                InputState.JumpButton = _controls.DebugInputs.DebugJump.phase;
#endif
                InputState.DashButton = _controls.GameplayDefault.Dash.phase;
                InputState.WallClingTrigger = _controls.GameplayDefault.WallCling.phase;

                InputState.directionalInputModifier = 
                    _controls.GameplayDefault.DigitalAxesModifier.phase is InputActionPhase.Performed &&
                    InputState.analogDeadzone;
            }
        }

        private void HandleAnalogMove(Vector2 in_input)
        {            
            InputState.analogDeadzone = in_input.magnitude <= GameplayControlConfigs.InputDeadZone;
            
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

        private void HandleDashButton()
        {
            if (_gameStateManager.ActiveUpdateType is not UpdateType.GamePlay)
                return;
            InputState.DashWindup = true;
        }

        public void SetInputStateToNeutral() =>
            InputState = new InputState();

        public void SetCharacterControlsActive(bool in_value) {
            if (in_value)
                _controls.Enable();
            else
                _controls.Disable();
        }
        
#if UNITY_EDITOR
        private void ToggleDebugMode()
        {
            GameStateManager.s_DebugMode = !GameStateManager.s_DebugMode;
            _characterStateController.Invulnerable = GameStateManager.s_DebugMode;
            Debug.Log("Debug Movement: " + GameStateManager.s_DebugMode);
        }
#endif
    }
}
