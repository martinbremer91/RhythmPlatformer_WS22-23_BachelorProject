using System.Collections.Generic;
using System.Linq;
using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class LevelManager : MonoBehaviour, IInit<GameStateManager>
    {
        private GameStateManager _gameStateManager;
        
        [SerializeField] private Transform _platformsParent;
        private List<Platform> _platforms = new();
        
        [SerializeField] private Transform _checkpointsParent;
        private List<Checkpoint> _checkpoints = new();

        [SerializeField] private Transform _hazardsParent;
        private List<Hazard> _hazards = new();

        [SerializeField] private Transform _dashCrystalsParent;
        private List<DashCrystal> _dashCrystals = new();

        private CharacterStateController _characterStateController;
        private Vector3 _currentSpawnPoint;
        private bool _spawnFacingLeft;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _characterStateController = in_gameStateManager.CharacterStateController;
            
            InitPlatforms();
            InitCheckpoints();
            InitHazards();
            InitDashCrystals();
            
            void InitPlatforms()
            {
                _platforms = _platformsParent.GetComponentsInChildren<Platform>().ToList();
                
                if (_platforms.Any())
                {
                    foreach (Platform platform in _platforms)
                        platform.Init(in_gameStateManager);
                }
            }

            void InitCheckpoints()
            {
                _currentSpawnPoint = _characterStateController.transform.position;
                _checkpoints = _checkpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
                
                if (_checkpoints.Any())
                {
                    foreach (Checkpoint checkpoint in _checkpoints)
                        checkpoint.Init(this, _characterStateController);
                }

                _characterStateController.Respawn += RespawnPlayer;
            }

            void InitHazards()
            {
                _hazards = _hazardsParent.GetComponentsInChildren<Hazard>().ToList();

                if (_hazards.Any())
                {
                    foreach (Hazard hazard in _hazards)
                        hazard.Init(in_gameStateManager);
                }
            }

            void InitDashCrystals()
            {
                _dashCrystals = _dashCrystalsParent.GetComponentsInChildren<DashCrystal>().ToList();

                if (_dashCrystals.Any())
                {
                    foreach (DashCrystal crystal in _dashCrystals)
                        crystal.Init(_characterStateController);
                }
            }
        }

        public void SetCurrentCheckPoint(Checkpoint in_checkpoint)
        {
            _currentSpawnPoint = in_checkpoint.SpawnPoint.position;
            _spawnFacingLeft = in_checkpoint.SpawnFacingLeft;
            _gameStateManager.CameraManager.OnSetCheckpoint(_currentSpawnPoint);
        }

        private void RespawnPlayer()
        {
            _characterStateController.transform.position = _currentSpawnPoint;
            _characterStateController.FacingLeft = _spawnFacingLeft;
        }

        private void OnDisable() =>
            _characterStateController.Respawn -= RespawnPlayer;
    }
}
