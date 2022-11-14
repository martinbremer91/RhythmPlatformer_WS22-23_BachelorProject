using System.Threading.Tasks;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class MovingPlatform : MonoBehaviour, IInit<GameStateManager, MovementRoutine>
    {
        private CharacterCollisionDetector _characterCollisionDetector;
        private CharacterStateController _characterStateController;
        private MovementRoutine _movementRoutine;

        [SerializeField] private BoxCollider2D _collider;

        private bool _isOneWayPlatform;
        
        public void Init(GameStateManager in_gameStateManager, MovementRoutine in_movementRoutine)
        {
            _isOneWayPlatform = gameObject.CompareTag("OneWayPlatform");
            if (_isOneWayPlatform)
                _characterStateController = in_gameStateManager.CharacterStateController;
            
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            _movementRoutine = in_movementRoutine;
        }
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.enabled && col.gameObject.CompareTag("Player"))
            {
                if (_isOneWayPlatform)
                    _characterCollisionDetector.OnOneWayPlatform = true;
                _movementRoutine.MovePlayerAsWell = true;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!_isOneWayPlatform)
                return;

            if (_characterStateController.CurrentCharacterState == CharacterState.Crouch)
                ExecuteFallThrough();
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (_isOneWayPlatform)
                    _characterCollisionDetector.OnOneWayPlatform = false;
                _movementRoutine.MovePlayerAsWell = false;
            }
        }

        private async void ExecuteFallThrough()
        {
            _characterCollisionDetector.OnOneWayPlatform = false;
            _movementRoutine.MovePlayerAsWell = false;

            _collider.enabled = false;
            await Task.Delay(500);
            _collider.enabled = true;
        }
    }
}
