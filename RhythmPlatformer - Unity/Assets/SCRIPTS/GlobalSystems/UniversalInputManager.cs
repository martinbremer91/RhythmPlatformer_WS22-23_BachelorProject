namespace GlobalSystems
{
    public static class UniversalInputManager
    {
        #region REFERENCES

        private static GameStateManager s_gameStateManager;

        #endregion
        
        private static DefaultControls s_controls;
        public static DefaultControls s_Controls
        {
            get { return s_controls ??= new DefaultControls(); }
        }

        public static void Init(GameStateManager in_gameStateManager)
        {
            s_gameStateManager = in_gameStateManager;
            s_Controls.UniversalInputs.PauseToggle.performed += _ => s_gameStateManager.ScheduleTogglePause();
        }
    }
}
