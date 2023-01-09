using System;
using System.Linq;
using System.Threading.Tasks;
using Gameplay;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using GlobalSystems;
using UnityEngine;
using UI_And_Menus;
using Utility_Scripts;

namespace GameplaySystems
{
    public class BeatManager : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES

        private GameStateManager _gameStateManager;
        private CharacterInput _characterInput;
        private CharacterStateController _characterStateController;
        private CharacterSpriteController _characterSpriteController;

        [SerializeField] private AudioSource[] _trackAudioSources;
        [SerializeField] private AudioLowPassFilter[] _trackLowPassFilters;

        [SerializeField] private float _startDelay;
        
        [SerializeField] private AudioSource _metronomeStrong;
        [SerializeField] private AudioSource _metronomeWeak;
        private TrackData _trackData;
#if UNITY_EDITOR
        public TrackData TrackData_editor => _trackData;
#endif
        private HUDController _hudController;
        private MusicUtilities _musicUtilities;

        #endregion

        #region VARIABLES

        private static BeatManager s_Instance;
        public UpdateType UpdateType => ~UpdateType.MenuTransition;

        public bool BeatActive;
        
        private int _activeSource;
        private int _nextSource => _activeSource == 0 ? 1 : 0;
        private double _nextTrackTime;
        
        private LoopPoints _loopPoints;

        [HideInInspector] public float _currentMusicVolume;
        [HideInInspector] public float _currentMetronomeVolume;
        
        [HideInInspector] public double BeatLength;
        [HideInInspector] public int BeatTracker;
        [HideInInspector] public int Meter;
        private double _nextBeatTime;
        private int _pausedBeat;

        [SerializeField] private bool _metronomeOn;
        public bool MetronomeOn
        {
            get => _metronomeOn;
            set =>
                _metronomeOn = _metronomeOnlyMode || value;
        }
        private bool _metronomeOnlyMode;
        private bool _pausedMetronome;

        private bool _unpauseSignal;
        
        #endregion

        public Action BeatAction;
        public Action EventBeatAction;

#if UNITY_EDITOR
        public int ActiveSource => _activeSource;
        [SerializeField] private bool _debugBeatOff;
        [SerializeField] private bool _debugBeatJumpOff;
#endif
        private struct LoopPoints
        {
            public double start;
            public double end;
        }

        #region INITIALIZATION

        private void OnEnable()
        {
            if (s_Instance != null && s_Instance != this)
                Destroy(gameObject);
        }

        private void OnDisable() =>
            SceneLoadManager.SceneUnloaded -= () => ExecuteLowPassFilterFade(false);

        private void OnApplicationFocus(bool focus) =>
            AudioListener.pause = !focus;

        public void Init(GameStateManager in_gameStateManager)
        {
            bool firstInit = false;

            if (s_Instance == null)
            {
                s_Instance = this;
                firstInit = true;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (firstInit)
            {
                _gameStateManager = in_gameStateManager;
                _musicUtilities = new MusicUtilities();
                SceneLoadManager.SceneUnloaded += () => ExecuteLowPassFilterFade(false);
            }

            _trackData = _gameStateManager.CurrentTrackData;
            _metronomeOnlyMode = _trackData.Clip == null;
            _trackAudioSources[0].clip = _trackData.Clip;
            _trackAudioSources[1].clip = _trackData.Clip;

            BeatActive = true;
            BeatTracker = 0;
            
            _characterInput = in_gameStateManager.CharacterInput;
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterSpriteController = in_gameStateManager.CharacterSpriteController;

            Meter = _trackData.Meter;

            BeatLength = 60 / (double) _trackData.BPM;
            double barLength = BeatLength * _trackData.Meter;

            _nextBeatTime = AudioSettings.dspTime + _startDelay;

            if (!_metronomeOnlyMode)
            {
                _loopPoints.start = barLength * _trackData.IntroBars;
                _loopPoints.end =
                    (Mathf.FloorToInt(_trackAudioSources[_activeSource].clip.length / (float)barLength)
                     - _trackData.TailBars) * barLength;

                _nextTrackTime = AudioSettings.dspTime + _loopPoints.end + _startDelay;

                _trackAudioSources[_activeSource].PlayScheduled(AudioSettings.dspTime + _startDelay);
                _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
            }
            else
                MetronomeOn = true;
            
            if (in_gameStateManager.HUDController != null)
            {
                _hudController = in_gameStateManager.HUDController;
                _hudController.InitializeHUD(in_gameStateManager, (float)BeatLength, _trackData);
            }
        }

        #endregion

        #region UPDATE AND PAUSE

        public void CustomUpdate()
        {
#if UNITY_EDITOR
            if (_debugBeatOff)
                return;
#endif
            double time = AudioSettings.dspTime;
            
            if (!_metronomeOnlyMode && time >= _nextTrackTime)
            {
                _activeSource = _nextSource;
                _nextTrackTime += _loopPoints.end - _loopPoints.start;
                _trackAudioSources[_activeSource].time = (float)_loopPoints.start;
                SetUpNextTrack();
                Debug.Log("BeatManager: switching audio sources");
            }

            if (time >= _nextBeatTime)
            {
                _nextBeatTime += BeatLength;
                BeatTracker = BeatTracker < _trackData.Meter ? BeatTracker + 1 : 1;

                bool gameplayActive = _gameStateManager.ActiveUpdateType == UpdateType.GamePlay;

                if (gameplayActive || _unpauseSignal)
                    BeatAction?.Invoke();

                if (_trackData.EventBeats.Any(b => b == BeatTracker))
                {
                    if (MetronomeOn)
                        _metronomeStrong.Play();

                    if (gameplayActive)
                    {
                        EventBeatAction?.Invoke();

                        if (!_characterStateController.Dead)
                            _characterInput.InputState.JumpCommand = true;
#if UNITY_EDITOR
                        if (_debugBeatJumpOff)
                            _characterInput.InputState.JumpCommand = false;
#endif
                    }
                } 
                else if (MetronomeOn)
                    _metronomeWeak.Play();
            }

            float progressPercentageInCurrentBeat = 1 - (float)(_nextBeatTime - time) / (float)BeatLength;
            int nextBeatClamped = BeatTracker + 1 > Meter ? 1 : BeatTracker + 1;
            bool nextBeatStrong = _trackData.EventBeats.Any(e => e == nextBeatClamped);
            _characterSpriteController.SetSilhouetteMaterialParameters(progressPercentageInCurrentBeat, nextBeatStrong);
            _hudController.UpdateHUD(BeatTracker);
        }

        public void RecordPausedBeatAndMetronome()
        {
            _pausedBeat = BeatTracker;
            _gameStateManager.PauseMenu.SetPausedBeatText(_pausedBeat);
            _pausedMetronome = MetronomeOn;
        }

        private async void SetUpNextTrack()
        {
            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;

            int tailMilliseconds = Mathf.RoundToInt(_trackData.TailBars * (float)BeatLength * Meter * 1000);
            await Task.Delay(tailMilliseconds);
            if (CheckQuitFunction())
                return;

            _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);

            SceneLoadManager.SceneUnloaded -= QuitFunction;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;
        }

        public async void ExecuteCountInAsync()
        {
            PauseMenu pauseMenu = _gameStateManager.PauseMenu;
            int countIn = 0;
            bool metronomeMute = _metronomeWeak.mute;

            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;

            while (!CheckQuitFunction() && BeatTracker != _trackData.Meter)
                await Task.Yield();
            if (CheckQuitFunction())
                return;

            SetMetronomeMute(false);
            MetronomeOn = true;
            while (!CheckQuitFunction() && BeatTracker != 1)
                await Task.Yield();
            if (CheckQuitFunction())
                return;

            UpdateCountInUi();

            while (!CheckQuitFunction() && BeatTracker != 2)
                await Task.Yield();
            if (CheckQuitFunction())
                return;

            int beatBeforeUnpause = _pausedBeat == 1 ? _trackData.Meter : _pausedBeat - 1;

            if (_pausedBeat != 1)
            {
                while (!CheckQuitFunction() && BeatTracker != 1)
                {
                    UpdateCountInUi();
                    await Task.Yield();
                }
                if (CheckQuitFunction())
                    return;
            }
            
            while (!CheckQuitFunction() && BeatTracker != beatBeforeUnpause)
            {
                UpdateCountInUi();
                await Task.Yield();
            }
            if (CheckQuitFunction())
                return;

            UpdateCountInUi();

            SetMetronomeMute(metronomeMute);
            MetronomeOn = _pausedMetronome;
            _unpauseSignal = true;
            BeatAction += _gameStateManager.TogglePause;
            ExecuteLowPassFilterFade(false);

            SceneLoadManager.SceneUnloaded -= QuitFunction;

            void UpdateCountInUi() {
                if (countIn != BeatTracker)
                {
                    pauseMenu.SetCountInText(BeatTracker);
                    countIn = BeatTracker;
                }
            }

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }
        }

        public void ExecuteLowPassFilterFade(bool in_lowPassOn) {
            if (!_trackLowPassFilters[0].enabled && !in_lowPassOn)
                return;

            float cutoffFrequency = 
                in_lowPassOn ? _gameStateManager.SoundConfigs.LowPassFilterFadeCutoffFrequency : 22000;
            
            _musicUtilities.FadeLowPassFilterAsync(_trackLowPassFilters, (float)BeatLength, 
                cutoffFrequency, !in_lowPassOn);
        }

        public void ExecuteVolumeFade(bool in_fadeOut) {
            float targetVolume = in_fadeOut ? 0 : 
                _gameStateManager.SoundConfigs.SoundPreferences.CurrentMusicVolume;

            _musicUtilities.FadeVolumeAsync(_trackAudioSources, 3, targetVolume);
        }

        #endregion

        #region SOUND CONFIGS

        public void SetMusicVolume(float in_value) {
            foreach (AudioSource source in _trackAudioSources)
                source.volume = in_value;
        }

        public void SetMetronomeVolume(float in_value) {
            _metronomeStrong.volume = in_value;
            _metronomeWeak.volume = in_value;
        }

        public void SetMusicMute(bool in_value) {
            foreach (AudioSource source in _trackAudioSources)
                source.mute = in_value;
        }

        public void SetMetronomeMute(bool in_value) {
            _pausedMetronome = !in_value;
            _metronomeStrong.mute = in_value;
            _metronomeWeak.mute = in_value;
        }

        #endregion
    }
}
