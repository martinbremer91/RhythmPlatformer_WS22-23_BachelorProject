using Gameplay;
using Interfaces_and_Enums;
using System;
using UnityEngine;

namespace GameplaySystems
{
    public class Checkpoint : MonoBehaviour, IInit<LevelManager, CharacterStateController>
    {
        private Guid CheckpointGuid;
        
        private LevelManager _levelManager;
        private CharacterStateController _characterStateController;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private bool _checkpointTouched;

        public Transform SpawnPoint;
        public bool SpawnFacingLeft;

        public void Init(LevelManager in_levelManager, CharacterStateController in_characterStateController)
        {
            _levelManager = in_levelManager;
            _characterStateController = in_characterStateController;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (_checkpointTouched || !col.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            _levelManager.SetCurrentCheckPoint(this);
            _spriteRenderer.color = Color.green;
            _checkpointTouched = true;
        }
    }
}
