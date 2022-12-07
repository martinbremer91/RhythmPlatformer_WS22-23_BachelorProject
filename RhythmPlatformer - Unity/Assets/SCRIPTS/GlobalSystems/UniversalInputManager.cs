using UI_And_Menus;

namespace GlobalSystems
{
    public static class UniversalInputManager
    {
        private static UiManager s_uiManager;
        private static DefaultControls s_controls = new();

        public static void Init(UiManager in_uiManager)
        {
            s_uiManager = in_uiManager;
            s_controls.UniversalInputs.MenuButton.performed += _ => s_uiManager.HandleMenuButtonPress();
            s_controls.Enable();
        }

        public static void SetUniversalControlsActive(bool in_value) {
            if (in_value)
                s_controls.Enable();
            else
                s_controls.Disable();
        }
    }
}
