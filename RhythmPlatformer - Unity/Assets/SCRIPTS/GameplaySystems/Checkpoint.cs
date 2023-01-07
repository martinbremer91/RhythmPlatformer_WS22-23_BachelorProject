using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class Checkpoint : MonoBehaviour, IInit<GameStateManager>
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ParticleSystem _flamesParticleSystemA;
        [SerializeField] private ParticleSystem _flamesParticleSystemB;

        private LevelManager _levelManager;
        private CharacterStateController _characterStateController;

        private PulseMaterialOverrides _pulseMaterialOverrides;

        private VisualsData _interactablesVisualsData;
        private LabeledColor _inactiveColor;
        private LabeledColor _activeColor;
        private Gradient _inactiveColorGradient;
        private Gradient _activeColorGradient;

        public bool CheckpointTouched;

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

            _inactiveColorGradient = new Gradient();
            _inactiveColorGradient.SetKeys(
                new GradientColorKey[] {
                new GradientColorKey(_inactiveColor.Color, 0),
                new GradientColorKey(_inactiveColor.Color, 1) },
                new GradientAlphaKey[] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(0, 1)
                });


            _activeColorGradient = new Gradient();
            _activeColorGradient.SetKeys(
                new GradientColorKey[] {
                new GradientColorKey(_activeColor.Color, 0),
                new GradientColorKey(_activeColor.Color, 1) },
                new GradientAlphaKey[] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(0, 1)
                });

            UpdateCheckpointVisuals();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (CheckpointTouched || !col.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            CheckpointTouched = true;
            _levelManager.SetCurrentCheckPoint(this);

            UpdateCheckpointVisuals();
        }

        public void UpdateCheckpointVisuals() {
            LabeledColor labeledColor = CheckpointTouched ? _activeColor : _inactiveColor;

            _pulseMaterialOverrides.SetBaseColor(labeledColor.Color);
            _pulseMaterialOverrides.SetSecondaryColor(labeledColor.HDRColor);
            UpdateFlameParticles();

            void UpdateFlameParticles() {
                Gradient gradient = CheckpointTouched ? _activeColorGradient : _inactiveColorGradient;

                ParticleSystem.ColorOverLifetimeModule flamesA =
                    _flamesParticleSystemA.colorOverLifetime;
                ParticleSystem.ColorOverLifetimeModule flamesB =
                    _flamesParticleSystemB.colorOverLifetime;

                flamesA.color = gradient;
                flamesB.color = gradient;
            }
        }
    }
}
