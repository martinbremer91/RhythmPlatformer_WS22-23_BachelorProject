using System;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class UpdateManager : MonoBehaviour, IInit<GameStateManager>, IRefreshable
    {
        private static UpdateManager s_Instance;

        private CameraManager CameraManager;
        private BeatManager BeatManager;
        private InputPlaybackManager InputPlaybackManager;
        private CharacterInput CharacterInput;
        private CharacterCollisionDetector CharacterCollisionDetector;
        private CharacterStateController CharacterStateController;
        private CharacterMovement CharacterMovement;

        private void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                (this as IRefreshable).RegisterRefreshable();
            }
            else
                Destroy(gameObject);
        }

        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

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

        public void SceneRefresh()
        {
            
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