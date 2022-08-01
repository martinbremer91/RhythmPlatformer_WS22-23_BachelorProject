using Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterInput : GameplayComponent
    {
        private SettingsConfigs settings;

        private DefaultControls controls;
        public static InputState InputState;

        private void Awake()
        {
            controls = new DefaultControls();

            controls.GameplayDefault.AnalogMove.performed += 
                ctx => HandleAnalogMove(ctx.ReadValue<Vector2>());
            controls.GameplayDefault.AnalogMove.canceled += 
                ctx => InputState.DirectionalInput = Vector2.zero;

            controls.GameplayDefault.DigitalMove.performed += 
                ctx => HandleDigitalMove(ctx.ReadValue<Vector2>());

            //temp
            controls.GameplayDefault.Jump.performed +=
                ctx => CharacterStateController.CurrentCharacterState = CharacterState.Rise;
            
            controls.Enable();
        }

        private void Start() => settings = ReferenceManager.Instance.Settings;

        private void Update()
        {
            InputState.DashButton = controls.GameplayDefault.Dash.phase;
            InputState.JumpButton = controls.GameplayDefault.Jump.phase;

            InputState.directionalInputModifier = 
                controls.GameplayDefault.DigitalAxesModifier.phase == InputActionPhase.Performed &&
                InputState.analogDeadzone;
        }

        private void HandleAnalogMove(Vector2 input) 
        {
            if (InputState.analogDeadzone = input.magnitude <= settings.InputDeadZone) 
            {
                InputState.DirectionalInput = Vector2.zero;
                return;
            }
            
            InputState.DirectionalInput = input;
        }

        private void HandleDigitalMove(Vector2 digitalInput)
        {
            if (InputState.analogDeadzone)
                InputState.DirectionalInput = digitalInput;
        }
    }
    
    public struct InputState
    {
        private Vector2 _directionalInput;
        public Vector2 DirectionalInput
        {
            get => _directionalInput * (directionalInputModifier ? .5f : 1);
            set => _directionalInput = value;
        }

        public bool directionalInputModifier;
        public bool analogDeadzone;
        
        public InputActionPhase DashButton;
        public InputActionPhase JumpButton;
    }
}
