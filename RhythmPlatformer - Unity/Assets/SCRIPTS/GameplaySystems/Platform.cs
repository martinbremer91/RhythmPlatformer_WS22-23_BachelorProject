using System.Threading.Tasks;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UI_And_Menus;
using UnityEngine;
using Utility_Scripts;

namespace GameplaySystems
{
    public class Platform : MonoBehaviour, IInit<GameStateManager>
    {
        private CharacterCollisionDetector _characterCollisionDetector;
        private CharacterStateController _characterStateController;
        [SerializeField] private MovementRoutine _movementRoutine;

        [SerializeField] private BoxCollider2D _collider;

        private bool _isOneWayPlatform;
        private bool _isMovingPlatform;
        
        public void Init(GameStateManager in_gameStateManager)
        {
            _isOneWayPlatform = gameObject.CompareTag(Constants.OneWayPlatform);
            if (_isOneWayPlatform)
                _characterStateController = in_gameStateManager.CharacterStateController;
            
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            
            if (_movementRoutine != null)
            {
                _isMovingPlatform = true;
                _movementRoutine.Init(in_gameStateManager);
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.enabled && col.gameObject.CompareTag(Constants.PlayerTag))
            {
                if (_isOneWayPlatform)
                    _characterCollisionDetector.OnOneWayPlatform = true;
                if (_isMovingPlatform)
                    _movementRoutine.MovePlayerAsWell = true;

                UiManager.ToggleDebugSymbol(true);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!_isOneWayPlatform || !collision.enabled || !collision.gameObject.CompareTag(Constants.PlayerTag))
                return;
            
            if (_characterStateController.CurrentCharacterState is CharacterState.Crouch)
                ExecuteFallThrough();
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                if (_isOneWayPlatform)
                    _characterCollisionDetector.OnOneWayPlatform = false;
                if (_isMovingPlatform)
                    _movementRoutine.MovePlayerAsWell = false;
                
                UiManager.ToggleDebugSymbol(false);
            }
        }

        private async void ExecuteFallThrough()
        {
            _characterCollisionDetector.OnOneWayPlatform = false;

            if (_isMovingPlatform)
                _movementRoutine.MovePlayerAsWell = false;

            _collider.enabled = false;
            await Task.Delay(500);
            _collider.enabled = true;
        }
    }
}
