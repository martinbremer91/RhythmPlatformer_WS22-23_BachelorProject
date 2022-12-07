using Gameplay;
using GameplaySystems;
using Interfaces_and_Enums;
using UI_And_Menus;
using Scriptable_Object_Scripts;
using Structs;
using System.Collections.Generic;
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
        public HUDController HUDController;
        public PulsingController PulsingController;

        [HideInInspector] public CameraManager CameraManager;
        [HideInInspector] public CameraManager CameraManagerAssistant;
        [HideInInspector] public LevelManager LevelManager;

        [HideInInspector] public CharacterInput CharacterInput;
        [HideInInspector] public CharacterCollisionDetector CharacterCollisionDetector;
        [HideInInspector] public CharacterStateController CharacterStateController;
        [HideInInspector] public CharacterMovement CharacterMovement;
        [HideInInspector] public CharacterSpriteController CharacterSpriteController;
        [HideInInspector] public CompanionSpriteController CompanionSpriteController;
        [HideInInspector] public CompanionFollow CompanionFollow;
        [HideInInspector] public TextAsset CameraBoundsData;

        public MovementConfigs MovementConfigs;
        public CompanionConfigs CompanionConfigs;
        public GameplayControlConfigs GameplayControlConfigs;
        public SoundConfigs SoundConfigs;
        [HideInInspector] public TrackData CurrentTrackData;

        #endregion

        public readonly List<IPhysicsPausable> PhysicsPausables = new();

        [HideInInspector] public SceneType LoadedSceneType;
        private UpdateType _activeUpdateTypeBuffer;
        [HideInInspector] public UpdateType ActiveUpdateType;

        private PlayerProgressData _playerProgressData;

        public static bool GameQuitting;

#if UNITY_EDITOR
        public static bool s_DebugMode;
#endif
        private void OnEnable()
        {
            if (s_Instance == null) {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (s_Instance != this) {
                Destroy(gameObject);
                return;
            }

            LoadPlayerProgress();
            LoadUserPrefs();
            (this as IRefreshable).RegisterRefreshable();
        }

        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

        private void OnApplicationFocus(bool focus) =>
            UpdateManagerPauseToggle(!focus);

        private void OnApplicationPause(bool pause) =>
            UpdateManagerPauseToggle(pause);

        private void OnApplicationQuit() =>
            GameQuitting = true;

        private void UpdateManagerPauseToggle(bool in_pause) {
            if (in_pause) {
                _activeUpdateTypeBuffer = ActiveUpdateType;
                ActiveUpdateType = UpdateType.Nothing;
                TogglePhysicsPause(true);
            } else if (_activeUpdateTypeBuffer is not UpdateType.Nothing) {
                ActiveUpdateType = _activeUpdateTypeBuffer;
                TogglePhysicsPause(false);
            }
        }

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
            BeatManager.Init(this);
            UiManager.Init(this);
            UniversalInputManager.Init(UiManager);
            PauseMenu.Init(this);
            PulsingController.Init(this);
            
            SceneInit();
            SceneLoadManager.RefreshGlobalObjects();
        }

        private void SceneInit()
        {
            switch (LoadedSceneType)
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
                PhysicsPausables.Clear();

                CameraManager.Init(this);
                LevelManager.Init(this);
                CharacterInput.Init(this);
                CharacterCollisionDetector.Init(this);
                CharacterStateController.Init(this);
                CharacterMovement.Init(this);
                CharacterSpriteController.Init(this);
                CompanionSpriteController.Init(this);
                CompanionFollow.Init(this);
            }
        }

        private void LoadPlayerProgress() {
            if (!_playerProgressData.LoadPlayerProgressData(ref _playerProgressData))
                _playerProgressData.SavePlayerProgressDataAsync(UiManager);
        }

        private void LoadUserPrefs() {
            if (!SoundConfigs.LoadSoundPreferences())
                SoundConfigs.SaveSoundPreferencesAsync(UiManager);
        }

        public void SceneRefresh()
        {
            // TODO: make sure to set ActiveUpdateType here!
        }

        public void ScheduleTogglePause()
        {            
            if (!BeatManager.BeatActive)
                BeatManager.ExecuteCountInAsync();
            else
                BeatManager.BeatAction += TogglePause;
        }

        public void TogglePause()
        { 
            BeatManager.BeatAction -= TogglePause;

            ActiveUpdateType = ActiveUpdateType == UpdateType.Paused ? UpdateType.GamePlay : UpdateType.Paused;
            bool paused = ActiveUpdateType == UpdateType.Paused;
            
            BeatManager.BeatActive = !paused;

            if (paused)
            {
                BeatManager.ExecuteLowPassFilterFade(true);
                BeatManager.RecordPausedBeatAndMetronome();
                BeatManager.MetronomeOn = false;
            }
            else
                PauseMenu.SetCountInText(0, false);

            TogglePhysicsPause(paused);
            CharacterSpriteController.OnTogglePause(paused);
            CompanionSpriteController.OnTogglePause(paused);
            PauseMenu.TogglePauseMenu(paused);
        }

        public void TogglePhysicsPause(bool in_paused)
        {
            foreach (IPhysicsPausable physicsPausable in PhysicsPausables)
                physicsPausable.TogglePausePhysics(in_paused);
        }
    }
}
