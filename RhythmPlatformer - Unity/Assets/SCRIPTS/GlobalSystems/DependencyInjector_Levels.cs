using Gameplay;
using GameplaySystems;
using Interfaces_and_Enums;

namespace GlobalSystems
{
    public class DependencyInjector_Levels : DependencyInjector
    {
        protected override SceneType GetSceneType() => SceneType.Level;

        public CameraManager CameraManager;
        public CharacterInput CharacterInput;
        public CharacterCollisionDetector CharacterCollisionDetector;
        public CharacterStateController CharacterStateController;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;
        public InputPlaybackManager InputPlaybackManager;

        public override void Init(GameStateManager in_gameStateManager)
        {
            in_gameStateManager.CameraManager = CameraManager;
            in_gameStateManager.CharacterInput = CharacterInput;
            in_gameStateManager.CharacterCollisionDetector = CharacterCollisionDetector;
            in_gameStateManager.CharacterStateController = CharacterStateController;
            in_gameStateManager.CharacterMovement = CharacterMovement;
            in_gameStateManager.CharacterSpriteController = CharacterSpriteController;
            in_gameStateManager.InputPlaybackManager = InputPlaybackManager;
            
            base.Init(in_gameStateManager);
        }
    }
}
