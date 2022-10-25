using Gameplay;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class UpdateManager : MonoBehaviour, IInit<GameStateManager>
    {
        private static UpdateManager Instance;

        private CameraManager CameraManager;
        private BeatManager BeatManager;
        private InputPlaybackManager InputPlaybackManager;
        private CharacterInput CharacterInput;
        private CharacterCollisionDetector CharacterCollisionDetector;
        private CharacterStateController CharacterStateController;
        private CharacterMovement CharacterMovement;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public void Init(GameStateManager in_gameStateManager)
        {
            BeatManager = in_gameStateManager.BeatManager;
            CameraManager = in_gameStateManager.CameraManager;
            
            InputPlaybackManager = in_gameStateManager.InputPlaybackManager;
            CharacterInput = in_gameStateManager.CharacterInput;
            CharacterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            CharacterStateController = in_gameStateManager.CharacterStateController;
            CharacterMovement = in_gameStateManager.CharacterMovement;
        }

        private void Update()
        {
            BeatManager.CustomUpdate();
            CameraManager.CustomUpdate();
        }
        
        private void FixedUpdate()
        {
            InputPlaybackManager.CustomUpdate();
            CharacterInput.CustomUpdate();
            CharacterCollisionDetector.CustomUpdate();
            CharacterStateController.CustomUpdate();
            CharacterMovement.CustomUpdate();
        }
    }
}
