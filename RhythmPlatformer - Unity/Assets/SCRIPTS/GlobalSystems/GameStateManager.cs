using Gameplay;
using GameplaySystems;
using Interfaces_and_Enums;
using Menus_and_Transitions;
using Scriptable_Object_Scripts;
using System;
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
        public PulsingController PulsingController;

        [HideInInspector] public CameraManager CameraManager;
        [HideInInspector] public LevelManager LevelManager;

        [HideInInspector] public CharacterInput CharacterInput;
        [HideInInspector] public CharacterCollisionDetector CharacterCollisionDetector;
        [HideInInspector] public CharacterStateController CharacterStateController;
        [HideInInspector] public CharacterMovement CharacterMovement;
        [HideInInspector] public CharacterSpriteController CharacterSpriteController;
        [HideInInspector] public InputPlaybackManager InputPlaybackManager;
        [HideInInspector] public TextAsset CameraBoundsData;

        public MovementConfigs MovementConfigs;
        public GameplayControlConfigs GameplayControlConfigs;

        #endregion

        public readonly List<IPhysicsPausable> PhysicsPausables = new();

        public static SceneType s_LoadedSceneType;
        public static UpdateType s_ActiveUpdateType;

        public Action<bool> TogglePauseEvent;

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
            PulsingController.Init(this);
            
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
                PhysicsPausables.Clear();

                CameraManager.Init(this);
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

            // Refactor TogglePhysicsPause when BeatManager has CountIn functionality
            TogglePhysicsPause(paused);
            TogglePauseEvent?.Invoke(paused);
            PauseMenu.TogglePauseMenu(paused);
        }

        public void TogglePhysicsPause(bool in_paused)
        {
            foreach (IPhysicsPausable physicsPausable in PhysicsPausables)
                physicsPausable.TogglePausePhysics(in_paused);
        }
    }
}
