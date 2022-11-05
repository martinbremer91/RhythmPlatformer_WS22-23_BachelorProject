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

        private CameraManager _cameraManager;
        private BeatManager _beatManager;
        private PulsingController _pulsingController;
        private InputPlaybackManager _inputPlaybackManager;
        private CharacterInput _characterInput;
        private CharacterCollisionDetector _characterCollisionDetector;
        private CharacterStateController _characterStateController;
        private CharacterMovement _characterMovement;

        private bool _updateActive;

        public readonly List<MovementRoutine> MovementRoutines = new();

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
            _beatManager = in_gameStateManager.BeatManager;
            _cameraManager = in_gameStateManager.CameraManager;
            _pulsingController = in_gameStateManager.PulsingController;
            
            _inputPlaybackManager = in_gameStateManager.InputPlaybackManager;
            _characterInput = in_gameStateManager.CharacterInput;
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
        }

        public void SceneRefresh() => 
            _updateActive = GameStateManager.s_LoadedSceneType == SceneType.Level;

        private void Update()
        {
            if (!_updateActive)
                return;
            
            UpdateType currentUpdateType = GameStateManager.s_ActiveUpdateType;

            if (MovementRoutine.s_UpdateType.HasFlag(currentUpdateType))
            {
                foreach (MovementRoutine routine in MovementRoutines)
                    routine.CustomUpdate();
            }
            
            if (_beatManager.UpdateType.HasFlag(currentUpdateType))
                _beatManager.CustomUpdate();
            if (_cameraManager.UpdateType.HasFlag(currentUpdateType))
                _cameraManager.CustomUpdate();
            if (_pulsingController.UpdateType.HasFlag(currentUpdateType))
                _pulsingController.CustomUpdate();
        }
        
        private void FixedUpdate()
        {
            if (!_updateActive)
                return;
            
            UpdateType currentUpdateType = GameStateManager.s_ActiveUpdateType;
            
            if (_inputPlaybackManager.UpdateType.HasFlag(currentUpdateType))
                _inputPlaybackManager.CustomUpdate();
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
