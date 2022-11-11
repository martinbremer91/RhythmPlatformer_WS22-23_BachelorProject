using System.Collections.Generic;
using System.Linq;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class LevelManager : MonoBehaviour, IInit<GameStateManager>
    {
        private GameStateManager _gameStateManager;
        
        public Transform MovementRoutineParent;
        private List<MovementRoutine> _movementRoutines = new();
        
        public Transform CheckpointsParent;
        private List<Checkpoint> _checkpoints = new();

        private CharacterStateController _characterStateController;
        private Vector3 _currentSpawnPoint;
        private bool _spawnFacingLeft;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _characterStateController = in_gameStateManager.CharacterStateController;
            
            _movementRoutines = MovementRoutineParent.GetComponentsInChildren<MovementRoutine>().ToList();
            if (_movementRoutines.Any())
                InitMovementRoutines();
            
            InitCheckpoints();
            
            void InitMovementRoutines()
            {
                UpdateType updateType = _movementRoutines[0].UpdateType;
            
                if (updateType != MovementRoutine.s_UpdateType)
                    Debug.LogError("MovementRoutine.UpdateType and MovementRoutine.s_UpdateType must match");

                foreach (MovementRoutine routine in _movementRoutines)
                    routine.Init(in_gameStateManager);
            }

            void InitCheckpoints()
            {
                _currentSpawnPoint = _characterStateController.transform.position;
                _checkpoints = CheckpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
                
                if (_checkpoints.Any())
                {
                    foreach (Checkpoint checkpoint in _checkpoints)
                        checkpoint.Init(this, _characterStateController);
                }

                _characterStateController.Respawn += RespawnPlayer;
            }
        }

        public void SetCurrentCheckPoint(Checkpoint in_checkpoint)
        {
            _currentSpawnPoint = in_checkpoint.SpawnPoint.position;
            _spawnFacingLeft = in_checkpoint.SpawnFacingLeft;
            _gameStateManager.CameraManager.OnSetCheckpoint(_currentSpawnPoint);
        }

        private void RespawnPlayer()
        {
            _characterStateController.transform.position = _currentSpawnPoint;
            _characterStateController.FacingLeft = _spawnFacingLeft;
        }

        private void OnDisable() =>
            _characterStateController.Respawn -= RespawnPlayer;
    }
}
