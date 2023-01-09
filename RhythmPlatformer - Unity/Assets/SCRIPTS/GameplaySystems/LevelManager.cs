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
        private List<CrystalBase> _crystals = new();

        [SerializeField] private LevelEnd _levelEnd;

        private CharacterStateController _characterStateController;
        private Vector3 _currentSpawnPoint;
        private bool _spawnFacingLeft;

        private CompanionMovement _companionFollow;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _characterStateController = in_gameStateManager.CharacterStateController;
            _companionFollow = in_gameStateManager.CompanionFollow;
            
            InitPlatforms();
            InitCheckpoints();
            InitHazards();
            InitDashCrystals();
            InitLevelEnd();
            
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
                SetSpawnPointToPosition(_characterStateController.transform.position);
                _checkpoints = _checkpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
                
                if (_checkpoints.Any())
                {
                    foreach (Checkpoint checkpoint in _checkpoints)
                        checkpoint.Init(in_gameStateManager);
                }

                in_gameStateManager.UiManager.CheckpointsMenu.SetCheckpoints(_checkpoints.ToArray());

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
                _crystals = _dashCrystalsParent.GetComponentsInChildren<CrystalBase>().ToList();

                if (_crystals.Any())
                {
                    foreach (CrystalBase crystal in _crystals)
                        crystal.Init(in_gameStateManager);
                }
            }

            void InitLevelEnd()
            {
                _levelEnd.Init(in_gameStateManager);
                in_gameStateManager.UiManager.CheckpointsMenu.SetLevelEnd(_levelEnd, in_gameStateManager, this);
            }
        }

        public void SetCurrentCheckPoint(Checkpoint in_checkpoint)
        {
            _currentSpawnPoint = in_checkpoint.SpawnPoint.position;
            _spawnFacingLeft = in_checkpoint.SpawnFacingLeft;
            _gameStateManager.CameraManager.OnSetCheckpoint(_currentSpawnPoint);
        }

        public void SetSpawnPointToPosition(Vector3 in_spawnPoint) => _currentSpawnPoint = in_spawnPoint;

        public void ResetCheckpoints()
        {
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                checkpoint.CheckpointTouched = false;
                checkpoint.UpdateCheckpointVisuals();
            }
        }

        private void RespawnPlayer()
        {
            _characterStateController.transform.position = _currentSpawnPoint;

            Vector2 companionPosOffset = _spawnFacingLeft ? 
                -_companionFollow.CharacterOffset : _companionFollow.CharacterOffset;
            _companionFollow.transform.position = 
                _currentSpawnPoint + (Vector3)companionPosOffset;

            _characterStateController.CanDash = true;
            _characterStateController.FacingLeft = _spawnFacingLeft;
        }

        private void OnDisable() =>
            _characterStateController.Respawn -= RespawnPlayer;
    }
}
