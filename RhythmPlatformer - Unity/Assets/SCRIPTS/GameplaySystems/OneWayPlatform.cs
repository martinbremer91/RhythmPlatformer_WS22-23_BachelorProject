using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class OneWayPlatform : MonoBehaviour, IInit<GameStateManager, MovementRoutine>
    {
        private CharacterCollisionDetector _characterCollisionDetector;
        private MovementRoutine _movementRoutine;

        public void Init(GameStateManager in_gameStateManager, MovementRoutine in_movementRoutine)
        {
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            _movementRoutine = in_movementRoutine;
        }
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.enabled && col.gameObject.CompareTag("Player"))
            {
                _characterCollisionDetector.OnOneWayPlatform = true;
                _movementRoutine.MovePlayerAsWell = true;
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.enabled && other.gameObject.CompareTag("Player"))
            {
                _characterCollisionDetector.OnOneWayPlatform = false;
                _movementRoutine.MovePlayerAsWell = false;
            }
        }
    }
}
