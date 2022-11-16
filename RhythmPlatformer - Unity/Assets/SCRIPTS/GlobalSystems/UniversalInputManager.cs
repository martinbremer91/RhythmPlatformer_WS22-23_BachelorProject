namespace GlobalSystems
{
    public static class UniversalInputManager
    {
        private static UiManager s_uiManager;
        private static DefaultControls s_controls;
        public static DefaultControls s_Controls
        {
            get { return s_controls ??= new DefaultControls(); }
        }

        public static void Init(UiManager in_uiManager)
        {
            s_uiManager = in_uiManager;
            s_Controls.UniversalInputs.MenuButton.performed += _ => s_uiManager.HandleMenuButtonPress();
            s_Controls.Enable();
        }
    }
}
