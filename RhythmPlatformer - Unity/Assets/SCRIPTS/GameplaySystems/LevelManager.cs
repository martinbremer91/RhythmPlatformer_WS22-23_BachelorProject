using System.Collections.Generic;
using System.Linq;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class LevelManager : MonoBehaviour, IInit<GameStateManager>
    {
        public Transform MovementRoutineParent;
        private List<MovementRoutine> _movementRoutines = new();

        public void Init(GameStateManager in_gameStateManager)
        {
            _movementRoutines = MovementRoutineParent.GetComponentsInChildren<MovementRoutine>().ToList();
            
            if (_movementRoutines.Any())
                InitMovementRoutines();
            
            void InitMovementRoutines()
            {
                UpdateType updateType = _movementRoutines[0].UpdateType;
            
                if (updateType != MovementRoutine.s_UpdateType)
                    Debug.LogError("MovementRoutine.UpdateType and MovementRoutine.s_UpdateType must match");

                foreach (MovementRoutine routine in _movementRoutines)
                    routine.Init(in_gameStateManager.UpdateManager);
            }
        }
    }
}
