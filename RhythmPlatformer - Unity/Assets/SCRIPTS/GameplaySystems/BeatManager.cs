using System;
using System.Linq;
using System.Threading.Tasks;
using Gameplay;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using GlobalSystems;
using UnityEngine;
using Menus_and_Transitions;
using Utility_Scripts;

namespace GameplaySystems
{
    public class BeatManager : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES

        private GameStateManager _gameStateManager;
        private CharacterInput _characterInput;

        private MusicUtilities _musicUtilities = new();
        [SerializeField] private AudioSource[] _trackAudioSources;

        [SerializeField] private float _startDelay;
        
        [SerializeField] private AudioSource _metronomeStrong;
        [SerializeField] private AudioSource _metronomeWeak;
        private TrackData _trackData;
#if UNITY_EDITOR
        public TrackData TrackData => _trackData;
#endif

        private PauseMenu _pauseMenu;

        #endregion

        #region VARIABLES

        private static BeatManager s_Instance;
        public UpdateType UpdateType => UpdateType.Always;

        public BeatState BeatState;
        
        private int _activeSource;
        private int _nextSource => _activeSource == 0 ? 1 : 0;
        private double _nextTrackTime;
        
        private LoopPoints _loopPoints;

        [HideInInspector] public float _currentMusicVolume;
        [HideInInspector] public float _currentMetronomeVolume;
        
        private double _beatLength;
        private double _nextBeatTime;
        private int _beatTracker;
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
        public int BeatTracker => _beatTracker;
        [SerializeField] private bool _debugBeatOff;
#endif
        private struct LoopPoints
        {
            public double start;
            public double end;
        }

        private void OnEnable()
        {
            if (s_Instance != null && s_Instance != this)
                Destroy(gameObject);
        }

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

            _trackData = _gameStateManager.CurrentTrackData;
            _metronomeOnlyMode = _trackData.Clip == null;
            _trackAudioSources[0].clip = _trackData.Clip;
            _trackAudioSources[1].clip = _trackData.Clip;

            // TODO: starting value will have to change to BeatState.StandBy in levels and BeatState.Off in menus
            BeatState = BeatState.Active;
            
            _characterInput = in_gameStateManager.CharacterInput;
            
            _beatLength = 60 / (double) _trackData.BPM;
            double barLength = _beatLength * _trackData.Meter;

            _nextBeatTime = AudioSettings.dspTime + _startDelay;

            if (!_metronomeOnlyMode)
            {
                _loopPoints.start = barLength * _trackData.IntroBars;
                _loopPoints.end =
                    (Mathf.FloorToInt(_trackAudioSources[_activeSource].clip.length / (float)barLength)
                     - _trackData.TailBars) * barLength;

                _nextTrackTime = AudioSettings.dspTime + _loopPoints.end - _loopPoints.start + _startDelay;

                _trackAudioSources[_activeSource].PlayScheduled(AudioSettings.dspTime + _startDelay);
                _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
            }
            else
                MetronomeOn = true;
        }

        public void CustomUpdate()
        {
            // TODO: Input playback currently broken (BeatManager not properly handled)
#if UNITY_EDITOR
            if (_debugBeatOff)
                return;
#endif
            
            double time = AudioSettings.dspTime;
            
            if (!_metronomeOnlyMode && time >= _nextTrackTime)
            {
                _activeSource = _nextSource;
                _nextTrackTime += _loopPoints.end - _loopPoints.start;
                _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
            }

            if (time >= _nextBeatTime)
            {
                _nextBeatTime += _beatLength;
                _beatTracker = _beatTracker < _trackData.Meter ? _beatTracker + 1 : 1;

                bool gameplayActive = _gameStateManager.ActiveUpdateType == UpdateType.GamePlay;

                if (gameplayActive || _unpauseSignal)
                    BeatAction?.Invoke();

                if (_trackData.EventBeats.Any(b => b == _beatTracker))
                {
                    if (MetronomeOn)
                        _metronomeStrong.Play();

                    if (gameplayActive)
                    {
                        EventBeatAction?.Invoke();
                        _characterInput.InputState.JumpCommand = true;
                    }
                } 
                else if (MetronomeOn)
                    _metronomeWeak.Play();
            }
        }

        public void RecordPausedBeatAndMetronome()
        {
            _pausedBeat = _beatTracker;
            _gameStateManager.PauseMenu.SetPausedBeatText(_pausedBeat);
            _pausedMetronome = MetronomeOn;
        } 

        // TODO: refactor this (maybe coroutine would be better here?)
        public async void ExecuteCountInAsync()
        {
            PauseMenu pauseMenu = _gameStateManager.PauseMenu;
            int countIn = 0;

            while (_beatTracker != _trackData.Meter && Time.unscaledDeltaTime > 0)
                await Task.Yield();

            if (Time.unscaledDeltaTime <= 0)
                return;

            MetronomeOn = true;
            while (_beatTracker != 1 && Time.unscaledDeltaTime > 0)
                await Task.Yield();

            if (Time.unscaledDeltaTime <= 0)
                return;

            UpdateCountInUi();

            while (_beatTracker != 2 && Time.unscaledDeltaTime > 0)
                await Task.Yield();

            if (Time.unscaledDeltaTime <= 0)
                return;

            int beatBeforeUnpause = _pausedBeat == 1 ? _trackData.Meter : _pausedBeat - 1;

            if (_pausedBeat != 1)
            {
                while (_beatTracker != 1 && Time.unscaledDeltaTime > 0)
                {
                    UpdateCountInUi();
                    await Task.Yield();
                }

                if (Time.unscaledDeltaTime <= 0)
                    return;
            }
            
            while (_beatTracker != beatBeforeUnpause && Time.unscaledDeltaTime > 0)
            {
                UpdateCountInUi();
                await Task.Yield();
            }

            if (Time.unscaledDeltaTime <= 0)
                return;

            UpdateCountInUi();

            MetronomeOn = _pausedMetronome;
            _unpauseSignal = true;
            BeatAction += _gameStateManager.TogglePause;

            void UpdateCountInUi() {
                if (countIn != _beatTracker)
                {
                    pauseMenu.SetCountInText(_beatTracker);
                    countIn = _beatTracker;
                }
            }
        }
    }
}
