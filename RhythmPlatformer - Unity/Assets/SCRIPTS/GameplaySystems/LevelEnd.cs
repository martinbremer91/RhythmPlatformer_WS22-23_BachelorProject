using System;
using GlobalSystems;
using Interfaces_and_Enums;
using UI_And_Menus;
using UnityEngine;
using Utility_Scripts;

namespace GameplaySystems
{
    public class LevelEnd : MonoBehaviour, IInit<GameStateManager>
    {
        private GameStateManager _gameStateManager;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ParticleSystem _flamesParticleSystem;
        [SerializeField] private ParticleSystem _burstParticleSystem;
        
        private string _levelToLoadName;
        private string LevelToLoadName => 
            _levelToLoadName == String.Empty ? Constants.MainMenu : _levelToLoadName;

        private UiManager _uiManager;

        private VisualsData _interactablesVisualsData;
        private PulseMaterialOverrides _pulseMaterialOverrides;

        private Gradient _inactiveColorGradient;
        private Gradient _activeColorGradient;
        private LabeledColor _inactiveColor;
        private LabeledColor _activeColor;

        private bool _levelEndTouched;

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;

            _uiManager = in_gameStateManager.UiManager;
            _levelToLoadName = in_gameStateManager.LevelSequenceData.GetLevelToLoadName();

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

            UpdateLevelEndVisuals();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (_gameStateManager.ActiveUpdateType is UpdateType.GamePlay && !_levelEndTouched && 
                collision.CompareTag(Constants.PlayerTag)) {
                _levelEndTouched = true;
                _burstParticleSystem.Play();
                UpdateLevelEndVisuals();
                LoadLevelToLoad();
            }
        }

        private void UpdateLevelEndVisuals() {
            LabeledColor labeledColor = _levelEndTouched ? _activeColor : _inactiveColor;

            _pulseMaterialOverrides.SetBaseColor(labeledColor.Color);
            _pulseMaterialOverrides.SetSecondaryColor(labeledColor.HDRColor);
            UpdateFlameParticles();

            void UpdateFlameParticles() {
                Gradient gradient = _levelEndTouched ? _activeColorGradient : _inactiveColorGradient;

                ParticleSystem.ColorOverLifetimeModule flamesA =
                    _flamesParticleSystem.colorOverLifetime;

                flamesA.color = gradient;
            }
        }

        private void LoadLevelToLoad() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(LevelToLoadName, _uiManager));
    }
}