using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class Checkpoint : MonoBehaviour, IInit<GameStateManager>
    {
        private LevelManager _levelManager;
        private CharacterStateController _characterStateController;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private PulseMaterialOverrides _pulseMaterialOverrides;

        private VisualsData _interactablesVisualsData;
        private LabeledColor _inactiveColor;
        private LabeledColor _activeColor;

        private bool _checkpointTouched;

        public Transform SpawnPoint;
        public bool SpawnFacingLeft;

        public void Init(GameStateManager in_gameStateManager)
        {
            _levelManager = in_gameStateManager.LevelManager;
            _characterStateController = in_gameStateManager.CharacterStateController;

            _pulseMaterialOverrides = new PulseMaterialOverrides(_spriteRenderer);

            _interactablesVisualsData = in_gameStateManager.VisualsData;
            LabeledColor[] interactablesColors = _interactablesVisualsData.InteractablesColors.ToArray();

            _inactiveColor = _interactablesVisualsData.GetColorByLabel(interactablesColors, "CheckpointInactive");
            _activeColor = _interactablesVisualsData.GetColorByLabel(interactablesColors, "CheckpointActive");

            _pulseMaterialOverrides.SetBaseColor(_inactiveColor.Color);
            _pulseMaterialOverrides.SetSecondaryColor(_inactiveColor.HDRColor);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (_checkpointTouched || !col.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            _levelManager.SetCurrentCheckPoint(this);

            _pulseMaterialOverrides.SetBaseColor(_activeColor.Color);
            _pulseMaterialOverrides.SetSecondaryColor(_activeColor.HDRColor);

            _checkpointTouched = true;
        }
    }
}
