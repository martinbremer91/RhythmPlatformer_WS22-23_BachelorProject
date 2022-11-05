using Interfaces_and_Enums;
using GlobalSystems;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System;

namespace GameplaySystems
{
    public class PulsingController : MonoBehaviour, IInit<GameStateManager>, IUpdatable
    {
        private static PulsingController s_Instance;

        public UpdateType UpdateType => UpdateType.GamePlay;

        [SerializeField] private PostProcessVolume _postProcessVolume;
        private Bloom _bloom;

        [SerializeField] private Vector2 _minMaxIntensity;
        [SerializeField] private float _fadeDuration;
        private bool _pulseFadeActive;
        private float _pulseFadeTimer;

        private void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
                Destroy(gameObject);
        }

        public void Init(GameStateManager in_gameStateManager)
        {
            if (!_postProcessVolume.profile.TryGetSettings<Bloom>(out _bloom))
                throw new Exception("Could not find bloom effect in post process volume component");

            in_gameStateManager.BeatManager.EventBeatAction += Pulse;
        }

        public void CustomUpdate()
        {
            if (!_pulseFadeActive)
                return;

            FadePulse();
        }

        private void Pulse()
        {
            _bloom.intensity.value = _minMaxIntensity.y;
            _pulseFadeActive = true;
            _pulseFadeTimer = 0;
        }

        private void FadePulse()
        {
            _pulseFadeTimer += Time.deltaTime;

            _bloom.intensity.value = 
                Mathf.Lerp(_minMaxIntensity.y, _minMaxIntensity.x, _pulseFadeTimer / _fadeDuration);

            if (_pulseFadeTimer >=_fadeDuration)
                _pulseFadeActive = false;
        }
    }
}
