using UnityEngine;
using UnityEngine.InputSystem;

namespace Structs
{
    public struct InputState
    {
        private Vector2 _directionalInput;
        public Vector2 DirectionalInput
        {
            get => _directionalInput * (directionalInputModifier ? .5f : 1);
            set => _directionalInput = value;
        }

        public bool DigitalUp, DigitalDown, DigitalLeft, DigitalRight;
        
        public bool directionalInputModifier;
        public bool analogDeadzone;
        
        public InputActionPhase DashButton;
        public InputActionPhase JumpButton;
        public InputActionPhase WallClingTrigger;
        
        public bool JumpCommand;
        public bool DashWindup;
    }
}