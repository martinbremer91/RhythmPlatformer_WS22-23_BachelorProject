using UnityEngine;

namespace Systems
{
    public static class InputManager
    {
        public static InputType s_InputType;
        
        
    }
    
    public enum InputType
    {
        KeyboardMouse,
        GamePad
    }
}
