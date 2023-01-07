using System.Threading.Tasks;
using GlobalSystems;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility_Scripts;

namespace UI_And_Menus
{
    public class UiManager : MonoBehaviour, IRefreshable, IInit<GameStateManager>
    {
        #region REFERENCES

        private static UiManager s_Instance;

        private GameStateManager _gameStateManager;
        [SerializeField] private GameObject _menuUI;
        [SerializeField] private GameObject _gameplayUI;
        [SerializeField] private GameObject _settingsUI;

        [SerializeField] private Image _fadeScreen;
        [SerializeField] private float _fadeDuration;

        [SerializeField] private GameObject _newGameButton, _resumeButton;

        [SerializeField] private SoundConfigs _soundConfigs;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _metronomeVolumeSlider;
        [SerializeField] private Toggle _musicMuteToggle;
        [SerializeField] private Toggle _metronomeMuteToggle;

        public GameObject LoadingScreen;
        public GameObject SaveInProgressText;

#if UNITY_EDITOR
        private static GameObject s_debugSymbol;
        [SerializeField] private GameObject _debugSymbol;
        public static void ToggleDebugSymbol(bool in_on) => s_debugSymbol.SetActive(in_on);
#endif

        private EventSystem _currentEventSystem => EventSystem.current;

        #endregion

        #region INITIALIZATION

        private void OnEnable()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            (this as IRefreshable).RegisterRefreshable();

#if UNITY_EDITOR
            s_debugSymbol = _debugSymbol;
#endif
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
            SyncSettingsMenuWithAudioSources();

            if (_gameStateManager.LoadedSceneType == SceneType.MainMenu)
                HandleOpenMainMenu();
        }

        public void SceneRefresh()
        {
            SceneType currentSceneType = _gameStateManager.LoadedSceneType;

            bool menuActive = currentSceneType == SceneType.MainMenu;
            _menuUI.SetActive(menuActive);
            _gameplayUI.SetActive(currentSceneType == SceneType.Level);
            _settingsUI.SetActive(false);

            if (menuActive)
                HandleOpenMainMenu();
        }

        #endregion

        #region BUTTON FUNCTIONS

        public void LoadMainMenuButton() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.MainMenu, this));

        public void HandleMenuButtonPress() {
            if (_settingsUI.activeSelf) {
                _soundConfigs.SaveSoundPreferencesAsync(this);
                _settingsUI.SetActive(false);
            }
            else if (_gameStateManager.LoadedSceneType == SceneType.Level)
                _gameStateManager.ScheduleTogglePause();
        }

        private void HandleOpenMainMenu() {
            _currentEventSystem.SetSelectedGameObject(_newGameButton);
        }

        public void HandleOpenSettings() =>
            _currentEventSystem.SetSelectedGameObject(_musicVolumeSlider.gameObject);

        public void HandleOpenCheckpointMenu() =>
            Debug.LogWarning("WARNING: Remember to actually write this code");

        public void HandleOpenPauseMenu() =>
            _currentEventSystem.SetSelectedGameObject(_resumeButton);

        public void HandleCloseSettings() {
            SceneType currentSceneType = _gameStateManager.LoadedSceneType;

            if (currentSceneType is SceneType.MainMenu)
                HandleOpenMainMenu();
            else if (currentSceneType is SceneType.Level)
                HandleOpenPauseMenu();
        }

        #endregion

        #region TRANSITIONS AND LOADING UI

        public async Task FadeDarkScreen(bool in_fadeScreenIn)
        {
            Color screenColor = _fadeScreen.color;

            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;
            
            if (in_fadeScreenIn)
            {
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, 0);
                _fadeScreen.gameObject.SetActive(true);
            }
            
            int timer = 0;
            Vector2 startAndEndAlphas = new(in_fadeScreenIn ? 0 : 1, in_fadeScreenIn ? 1 : 0);

            while (!CheckQuitFunction() && timer <= _fadeDuration * 1000 + 100)
            {
                int deltaTimeMilliseconds = Mathf.RoundToInt(1000 * Time.deltaTime);
                await Task.Delay(deltaTimeMilliseconds);
                
                timer += deltaTimeMilliseconds;

                float alpha = 
                    Mathf.Lerp(startAndEndAlphas.x, startAndEndAlphas.y, timer / (_fadeDuration * 1000));
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, alpha);
            }

            if (CheckQuitFunction())
                return;

            _fadeScreen.color = in_fadeScreenIn ? new Color(screenColor.r, screenColor.g, screenColor.b, 1) : 
                _fadeScreen.color = new Color(screenColor.r, screenColor.g, screenColor.b, 0);

            if (!in_fadeScreenIn)
                _fadeScreen.gameObject.SetActive(false);

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }
        }

        #endregion

        #region SETTINGS FUNCTIONS

        private void SyncSettingsMenuWithAudioSources() {
            SoundPreferencesData soundPrefs = _soundConfigs.SoundPreferences;

            _musicMuteToggle.SetIsOnWithoutNotify(soundPrefs.MusicMuted);
            _metronomeMuteToggle.SetIsOnWithoutNotify(soundPrefs.MetronomeMuted);
            _musicVolumeSlider.SetValueWithoutNotify(soundPrefs.CurrentMusicVolume);
            _metronomeVolumeSlider.SetValueWithoutNotify(soundPrefs.CurrentMetronomeVolume);
        }

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
