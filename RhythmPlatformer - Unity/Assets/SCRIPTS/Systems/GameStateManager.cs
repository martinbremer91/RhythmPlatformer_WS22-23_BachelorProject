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

        public UpdateManager UpdateManager;
        public PauseMenu PauseMenu;

        public CameraManager CameraManager;
        public BeatManager BeatManager;

        public CharacterInput CharacterInput;
        public CharacterCollisionDetector CharacterCollisionDetector;
        public CharacterStateController CharacterStateController;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;

        public InputPlaybackManager InputPlaybackManager;

        public MovementConfigs MovementConfigs;
        public GameplayControlConfigs GameplayControlConfigs;

        #endregion
        
        [SerializeField] private UpdateType _startUpdateType;
        private static UpdateType s_activeUpdateType;
        public static UpdateType s_ActiveUpdateType;

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
            
            UpdateManager.Init(this);
            
            UniversalInputManager.Init(this);
            BeatManager.Init(this);
            
            // TODO: add logic that checks if character is present in scene
            // TODO: externalize these init calls
            
            InputPlaybackManager.Init(this);
            CharacterInput.Init(this);
            CharacterCollisionDetector.Init(this);
            CharacterStateController.Init(this);
            CharacterMovement.Init(this);
            CharacterSpriteController.Init(this);
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
