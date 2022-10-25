using System;
using Gameplay;
using Interfaces;
using Scriptable_Object_Scripts;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        private static GameStateManager s_Instance;
        
        #region REFERENCES
        
        public PauseMenu PauseMenu;

        public BeatManager BeatManager;

        public CharacterInput CharacterInput;
        public CharacterCollisionDetector CharacterCollisionDetector;
        public CharacterStateController CharacterStateController;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;

        public MovementConfigs MovementConfigs;
        public GameplayControlConfigs GameplayControlConfigs;

        #endregion
        
        [SerializeField] private UpdateType _startUpdateType;
        private static UpdateType s_activeUpdateType;
        public static UpdateType s_ActiveUpdateType
        {
            get => s_activeUpdateType;
            set
            {
                if (s_activeUpdateType == value)
                    return;
                
                UpdateType oldValue = s_activeUpdateType;
                
                if (oldValue == UpdateType.Paused)
                    TogglePause?.Invoke(false);
                if (value == UpdateType.Paused)
                    TogglePause?.Invoke(true);

                s_activeUpdateType = value;
            }
        }

        public static Action<bool> TogglePause;

#if UNITY_EDITOR
        public static bool s_DebugMode;
#endif

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            s_ActiveUpdateType = _startUpdateType;
            
            UniversalInputManager.Init(this);
            
            BeatManager.Init(this);
            
            // TODO: add logic that checks if character is present in scene
            // TODO: externalize these init calls
            
            CharacterInput.Init(this);
            CharacterCollisionDetector.Init(this);
            CharacterStateController.Init(this);
            CharacterMovement.Init(this);
            CharacterSpriteController.Init(this);
        }
    }
}
