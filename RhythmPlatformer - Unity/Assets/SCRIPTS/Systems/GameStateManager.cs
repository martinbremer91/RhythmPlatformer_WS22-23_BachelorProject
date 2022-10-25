using Gameplay;
using Interfaces;
using Scriptable_Object_Scripts;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager s_Instance;

        #region REFERENCES

        [SerializeField] private DependencyInjector _dependencyInjector;
        
        public UpdateManager UpdateManager;
        public PauseMenu PauseMenu;

        public BeatManager BeatManager;
        [HideInInspector] public CameraManager CameraManager;

        [HideInInspector] public CharacterInput CharacterInput;
        [HideInInspector] public CharacterCollisionDetector CharacterCollisionDetector;
        [HideInInspector] public CharacterStateController CharacterStateController;
        [HideInInspector] public CharacterMovement CharacterMovement;
        [HideInInspector] public CharacterSpriteController CharacterSpriteController;
        [HideInInspector] public InputPlaybackManager InputPlaybackManager;

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
            
            _dependencyInjector.Init(this);
            
            // TODO: this next block of init functions are all for DontDestroyOnLoads
            // TODO: (add check to see if they're already initialized)
            UpdateManager.Init(this);
            UniversalInputManager.Init(this);
            BeatManager.Init(this);
            
            // TODO: add switch that only initializes elements of loaded scene
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
