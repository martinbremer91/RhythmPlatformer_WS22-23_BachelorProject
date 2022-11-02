using Gameplay;
using GameplaySystems;
using Interfaces_and_Enums;
using Menus_and_Transitions;
using Scriptable_Object_Scripts;
using UnityEngine;

namespace GlobalSystems
{
    public class GameStateManager : MonoBehaviour, IInit<DependencyInjector>, IRefreshable
    {
        public static GameStateManager s_Instance;

        #region REFERENCES

        [HideInInspector] public DependencyInjector DependencyInjector;
        
        public UpdateManager UpdateManager;
        public PauseMenu PauseMenu;

        public BeatManager BeatManager;
        public UiManager UiManager;
        [HideInInspector] public CameraManager CameraManager;
        [HideInInspector] public LevelManager LevelManager;

        [HideInInspector] public CharacterInput CharacterInput;
        [HideInInspector] public CharacterCollisionDetector CharacterCollisionDetector;
        [HideInInspector] public CharacterStateController CharacterStateController;
        [HideInInspector] public CharacterMovement CharacterMovement;
        [HideInInspector] public CharacterSpriteController CharacterSpriteController;
        [HideInInspector] public InputPlaybackManager InputPlaybackManager;

        public MovementConfigs MovementConfigs;
        public GameplayControlConfigs GameplayControlConfigs;

        #endregion

        public static SceneType s_LoadedSceneType;
        
        private static UpdateType s_activeUpdateType;
        public static UpdateType s_ActiveUpdateType;

#if UNITY_EDITOR
        public static bool s_DebugMode;
#endif
        private void OnEnable() => (this as IRefreshable).RegisterRefreshable();

        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

        public void Init(DependencyInjector in_dependencyInjector)
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DependencyInjector = in_dependencyInjector;
            DependencyInjector.Init(this);
            
            UpdateManager.Init(this);
            UniversalInputManager.Init(this);
            BeatManager.Init(this);
            UiManager.Init();
            
            SceneInit();
            SceneLoadManager.RefreshGlobalObjects();
        }

        private void SceneInit()
        {
            switch (s_LoadedSceneType)
            {
                case SceneType.MainMenu:
                    InitMainMenuScene();
                    break;
                case SceneType.Level:
                    InitLevelScene();
                    break;
            }

            void InitMainMenuScene(){}
            
            void InitLevelScene()
            {
                LevelManager.Init(this);
                InputPlaybackManager.Init(this);
                CharacterInput.Init(this);
                CharacterCollisionDetector.Init(this);
                CharacterStateController.Init(this);
                CharacterMovement.Init(this);
                CharacterSpriteController.Init(this);
            }
        }

        public void SceneRefresh()
        {
            // TODO: make sure to set s_ActiveUpdateType here!
        }

        public void ScheduleTogglePause()
        {
            if (BeatManager.BeatState == BeatState.Off)
                TogglePause();
            else
                BeatManager.BeatAction += TogglePause;
        } 

        private void TogglePause()
        {
            BeatManager.BeatAction -= TogglePause;
            
            s_ActiveUpdateType = s_ActiveUpdateType == UpdateType.Paused ? UpdateType.GamePlay : UpdateType.Paused;
            bool paused = s_ActiveUpdateType == UpdateType.Paused;
            
            BeatManager.BeatState = BeatManager.BeatState == BeatState.Off ? BeatState.Off :
                paused ? BeatState.Standby : BeatState.Active;
            
            PauseMenu.TogglePauseMenu(paused);
        }
    }
}
