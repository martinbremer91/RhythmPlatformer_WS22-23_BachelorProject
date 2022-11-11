using System.Collections.Generic;
using System.Linq;
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

        [HideInInspector] public Vector3 CurrentSpawnPoint;
        [HideInInspector] public bool SpawnFacingLeft;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            
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
                CurrentSpawnPoint = in_gameStateManager.CharacterMovement.transform.position;
                
                foreach (Checkpoint checkpoint in _checkpoints)
                    checkpoint.Init(this);
            }
        }

        public void SetCurrentCheckPoint(Checkpoint in_checkpoint)
        {
            CurrentSpawnPoint = in_checkpoint.SpawnPoint.position;
            SpawnFacingLeft = in_checkpoint.SpawnFacingLeft;
            _gameStateManager.CameraManager.OnSetCheckpoint(CurrentSpawnPoint);
        }
    }
}
