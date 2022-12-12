using System.Collections.Generic;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class UpdateManager : MonoBehaviour, IInit<GameStateManager>, IRefreshable
    {
        private static UpdateManager s_Instance;

        private GameStateManager _gameStateManager;
        private CameraManager _cameraManager;
        private CameraManager _cameraManagerAssistant;
        private BeatManager _beatManager;
        private PulsingController _pulsingController;
        private CharacterInput _characterInput;
        private CharacterCollisionDetector _characterCollisionDetector;
        private CharacterStateController _characterStateController;
        private CharacterMovement _characterMovement;
        private CompanionFollow _companionFollow;

        private bool _updateActive;

        public readonly List<MovementRoutine> MovementRoutines = new();

#if UNITY_EDITOR
        [SerializeField] private float _debugTimeScale = 1;
#endif

        private void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                (this as IRefreshable).RegisterRefreshable();
            }
            else
                Destroy(gameObject);
        }

        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _beatManager = in_gameStateManager.BeatManager;
            _cameraManager = in_gameStateManager.CameraManager;
            _cameraManagerAssistant = in_gameStateManager.CameraManagerAssistant;
            _pulsingController = in_gameStateManager.PulsingController;
            
            _characterInput = in_gameStateManager.CharacterInput;
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
            _companionFollow = in_gameStateManager.CompanionFollow;

#if UNITY_EDITOR
            Time.timeScale = _debugTimeScale <= 0 ? 1 :_debugTimeScale;
#endif
        }

        public void SceneRefresh() => 
            _updateActive = _gameStateManager.LoadedSceneType is SceneType.Level;

        private void Update()
        {
            if (!_updateActive)
                return;
            if (_gameStateManager.ActiveUpdateType is UpdateType.Nothing)
                return;
            
            UpdateType currentUpdateType = _gameStateManager.ActiveUpdateType;

            if (MovementRoutine.s_UpdateType.HasFlag(currentUpdateType))
            {
                foreach (MovementRoutine routine in MovementRoutines)
                    routine.CustomUpdate();
            }
            
            if (_beatManager.UpdateType.HasFlag(currentUpdateType))
                _beatManager.CustomUpdate();
            if (_cameraManager.UpdateType.HasFlag(currentUpdateType))
            {
                if (_cameraManagerAssistant.isActiveAndEnabled)
                    _cameraManagerAssistant.CustomUpdate();
                _cameraManager.CustomUpdate();
            }
            if (_companionFollow.UpdateType.HasFlag(currentUpdateType))
                _companionFollow.CustomUpdate();
            if (_pulsingController.UpdateType.HasFlag(currentUpdateType))
                _pulsingController.CustomUpdate();
        }
        
        private void FixedUpdate()
        {
            if (!_updateActive)
                return;
            if (_gameStateManager.ActiveUpdateType is UpdateType.Nothing)
                return;

            UpdateType currentUpdateType = _gameStateManager.ActiveUpdateType;

            if (_characterInput.UpdateType.HasFlag(currentUpdateType))
                _characterInput.CustomUpdate();
            if (_characterCollisionDetector.UpdateType.HasFlag(currentUpdateType))
                _characterCollisionDetector.CustomUpdate();
            if (_characterStateController.UpdateType.HasFlag(currentUpdateType))
                _characterStateController.CustomUpdate();
            if (_characterMovement.UpdateType.HasFlag(currentUpdateType))
                _characterMovement.CustomUpdate();
        }
    }
}
