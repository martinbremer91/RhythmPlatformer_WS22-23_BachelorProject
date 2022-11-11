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
        [HideInInspector] public Vector3 CurrentSpawnPoint;
        [HideInInspector] public bool SpawnFacingLeft;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _characterStateController = in_gameStateManager.CharacterStateController;
            
            _movementRoutines = MovementRoutineParent.GetComponentsInChildren<MovementRoutine>().ToList();
            if (_movementRoutines.Any())
                InitMovementRoutines();
            
            _checkpoints = CheckpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
            if (_checkpoints.Any())
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
                CurrentSpawnPoint = _characterStateController.transform.position;
                
                foreach (Checkpoint checkpoint in _checkpoints)
                    checkpoint.Init(this);

                _characterStateController.Respawn += RespawnPlayer;
            }
        }

        public void SetCurrentCheckPoint(Checkpoint in_checkpoint)
        {
            CurrentSpawnPoint = in_checkpoint.SpawnPoint.position;
            SpawnFacingLeft = in_checkpoint.SpawnFacingLeft;
            _gameStateManager.CameraManager.OnSetCheckpoint(CurrentSpawnPoint);
        }

        private void RespawnPlayer()
        {
            _characterStateController.transform.position = CurrentSpawnPoint;
            _characterStateController.FacingLeft = SpawnFacingLeft;
        }
    }
}
