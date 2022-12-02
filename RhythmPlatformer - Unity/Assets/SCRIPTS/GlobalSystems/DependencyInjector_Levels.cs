using Gameplay;
using GameplaySystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GlobalSystems
{
    public class DependencyInjector_Levels : DependencyInjector
    {
        protected override SceneType GetSceneType() => SceneType.Level;
        protected override UpdateType GetUpdateType() => UpdateType.GamePlay;

        public CameraManager CameraManager;
        public CameraManager CameraManagerAssistant;
        public HUDController HUDController;
        public TextAsset CameraBoundsData;
        public LevelManager LevelManager;
        public CharacterInput CharacterInput;
        public CharacterCollisionDetector CharacterCollisionDetector;
        public CharacterStateController CharacterStateController;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;
        public CompanionSpriteController CompanionSpriteController;
        public CompanionFollow CompanionFollow;

        public override void Init(GameStateManager in_gameStateManager)
        {
            in_gameStateManager.CameraManager = CameraManager;
            in_gameStateManager.CameraManagerAssistant = CameraManagerAssistant;
            in_gameStateManager.HUDController = HUDController;
            in_gameStateManager.CameraBoundsData = CameraBoundsData;
            in_gameStateManager.LevelManager = LevelManager;
            in_gameStateManager.CharacterInput = CharacterInput;
            in_gameStateManager.CharacterCollisionDetector = CharacterCollisionDetector;
            in_gameStateManager.CharacterStateController = CharacterStateController;
            in_gameStateManager.CharacterMovement = CharacterMovement;
            in_gameStateManager.CharacterSpriteController = CharacterSpriteController;
            in_gameStateManager.CompanionSpriteController = CompanionSpriteController;
            in_gameStateManager.CompanionFollow = CompanionFollow;
            
            base.Init(in_gameStateManager);
        }
    }
}
