namespace Systems
{
    public static class UniversalInputManager
    {
        #region REFERENCES

        private static PauseMenu s_pauseMenu;

        #endregion

        private static DefaultControls _controls;
        public static DefaultControls s_Controls
        {
            get { return _controls ??= new DefaultControls(); }
        }

        public static void Init(GameStateManager in_gameStateManager)
        {
            s_Controls.UniversalInputs.PauseToggle.performed += _ => s_pauseMenu.TogglePause();
            s_pauseMenu = in_gameStateManager.PauseMenu;
        }
    }
}
