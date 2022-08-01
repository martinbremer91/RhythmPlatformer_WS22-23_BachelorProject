using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterInput : GameplayComponent
    {
        private DefaultControls controls;

        public static InputState InputState;

        private void Awake()
        {
            controls = new DefaultControls();

            controls.GameplayDefault.AnalogMove.performed += 
                ctx => InputState.DirectionalInput = ctx.ReadValue<Vector2>();
            controls.GameplayDefault.AnalogMove.canceled += 
                ctx => InputState.DirectionalInput = Vector2.zero;

            controls.GameplayDefault.DigitalMove.performed += 
                ctx => HandleDigitalMove(ctx.ReadValue<Vector2>());

            //temp
            controls.GameplayDefault.Jump.performed +=
                ctx => CharacterStateController.CurrentCharacterState = CharacterState.Rise;
            
            controls.Enable();
        }

        private void Update()
        {
            InputState.DashButton = controls.GameplayDefault.Dash.phase;
            InputState.JumpButton = controls.GameplayDefault.Jump.phase;

            InputState.directionalInputModifier = 
                controls.GameplayDefault.DigitalAxesModifier.phase == InputActionPhase.Performed &&
                controls.GameplayDefault.AnalogMove.phase == InputActionPhase.Waiting;
        }

        private void HandleDigitalMove(Vector2 digitalInput)
        {
            if (controls.GameplayDefault.AnalogMove.phase == InputActionPhase.Waiting)
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
        
        public InputActionPhase DashButton;
        public InputActionPhase JumpButton;
    }
}
