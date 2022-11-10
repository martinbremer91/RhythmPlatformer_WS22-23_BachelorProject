using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class OneWayPlatform : MonoBehaviour, IInit<GameStateManager>
    {
        private CharacterCollisionDetector _characterCollisionDetector;

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
        }
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.enabled && col.gameObject.CompareTag("Player"))
                _characterCollisionDetector.OnOneWayPlatform = true;
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.enabled && other.gameObject.CompareTag("Player"))
                _characterCollisionDetector.OnOneWayPlatform = false;
        }
    }
}
