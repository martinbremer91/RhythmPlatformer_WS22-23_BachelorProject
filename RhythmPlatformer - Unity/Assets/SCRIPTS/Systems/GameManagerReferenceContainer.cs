using Gameplay;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class GameManagerReferenceContainer : MonoBehaviour, IInit<GameStateManager>
    {
        public CameraManager CameraManager;
        public CharacterInput CharacterInput;
        public CharacterCollisionDetector CharacterCollisionDetector;
        public CharacterStateController CharacterStateController;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;
        public InputPlaybackManager InputPlaybackManager;

        public void Init(GameStateManager in_gameStateManager)
        {
            if (in_gameStateManager == GameStateManager.s_Instance)
            {
                CameraManager = in_gameStateManager.CameraManager;
                CharacterInput = in_gameStateManager.CharacterInput;
                CharacterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
                CharacterStateController = in_gameStateManager.CharacterStateController;
                CharacterMovement = in_gameStateManager.CharacterMovement;
                CharacterSpriteController = in_gameStateManager.CharacterSpriteController;
                InputPlaybackManager = in_gameStateManager.InputPlaybackManager;
            }
            
            Destroy(gameObject);
        }
    }
}
