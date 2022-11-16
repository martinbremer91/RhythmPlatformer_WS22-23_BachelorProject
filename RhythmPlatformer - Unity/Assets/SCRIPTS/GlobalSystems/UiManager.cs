using System.Threading.Tasks;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.UI;
using Utility_Scripts;

namespace GlobalSystems
{
    public class UiManager : MonoBehaviour, IRefreshable, IInit<GameStateManager>
    {
        private static UiManager s_Instance;

        private GameStateManager _gameStateManager;
        [SerializeField] private GameObject _menuUI;
        [SerializeField] private GameObject _gameplayUI;
        [SerializeField] private GameObject _settingsUI;

        [SerializeField] private Image _fadeScreen;
        [SerializeField] private float _fadeDuration;

        [SerializeField] private SoundConfigs _soundConfigs;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _metronomeVolumeSlider;
        [SerializeField] private Toggle _musicMuteToggle;
        [SerializeField] private Toggle _metronomeMuteToggle;

        public GameObject PlayIcon;
        public GameObject RecordIcon;

        private void OnEnable()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            (this as IRefreshable).RegisterRefreshable();
        } 
        
        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

        public void Init(GameStateManager in_gameStateManager)
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _gameStateManager = in_gameStateManager;
        }
        
        public void SceneRefresh()
        {
            SceneType currentSceneType = _gameStateManager.LoadedSceneType;

            _menuUI.SetActive(currentSceneType == SceneType.MainMenu);
            _gameplayUI.SetActive(currentSceneType == SceneType.Level);
            _settingsUI.SetActive(false);
        }
        
        public void LoadMainMenuButton() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.MainMenu));

        public void HandleMenuButtonPress() {
            if (_settingsUI.activeSelf) {
                _soundConfigs.SaveSoundPreferencesAsync();
                _settingsUI.SetActive(false);
            }
            else if (_gameStateManager.LoadedSceneType == SceneType.Level)
                _gameStateManager.ScheduleTogglePause();
        }

        public async Task FadeDarkScreen(bool in_fadeScreenIn)
        {
            Color screenColor = _fadeScreen.color;
            
            if (in_fadeScreenIn)
            {
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, 0);
                _fadeScreen.gameObject.SetActive(true);
            }
            
            int timer = 0;
            Vector2 startAndEndAlphas = new(in_fadeScreenIn ? 0 : 1, in_fadeScreenIn ? 1 : 0);

            while (timer <= _fadeDuration * 1000 + 100 && Time.deltaTime > 0)
            {
                int deltaTimeMilliseconds = Mathf.RoundToInt(1000 * Time.deltaTime);
                await Task.Delay(deltaTimeMilliseconds);
                
                timer += deltaTimeMilliseconds;

                float alpha = 
                    Mathf.Lerp(startAndEndAlphas.x, startAndEndAlphas.y, timer / (_fadeDuration * 1000));
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, alpha);
            }

            if (Time.deltaTime <= 0)
                return;

            _fadeScreen.color = in_fadeScreenIn ? new Color(screenColor.r, screenColor.g, screenColor.b, 1) : 
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, 0);

            if (!in_fadeScreenIn)
                _fadeScreen.gameObject.SetActive(false);
        }

        #region SETTINGS FUNCTIONS

        public void HandleMusicVolumeChange() {
            float sliderValue = _musicVolumeSlider.value;
            _gameStateManager.BeatManager.SetMusicVolume(sliderValue);
            _soundConfigs.SoundPreferences.CurrentMusicVolume = sliderValue;
        }

        public void HandleMetronomeVolumeChange() {
            float sliderValue = _metronomeVolumeSlider.value;
            _gameStateManager.BeatManager.SetMetronomeVolume(sliderValue);
            _soundConfigs.SoundPreferences.CurrentMetronomeVolume = sliderValue;
        }

        public void HandleMusicMuteChange() {
            bool toggleValue = _musicMuteToggle.isOn;
            _gameStateManager.BeatManager.SetMusicMute(toggleValue);
            _soundConfigs.SoundPreferences.MusicMuted = toggleValue;
        }

        public void HandleMetronomeMuteChange() {
            bool toggleValue = _metronomeMuteToggle.isOn;
            _gameStateManager.BeatManager.SetMetronomeMute(toggleValue);
            _soundConfigs.SoundPreferences.MetronomeMuted = toggleValue;
        }

        #endregion
    }
}
