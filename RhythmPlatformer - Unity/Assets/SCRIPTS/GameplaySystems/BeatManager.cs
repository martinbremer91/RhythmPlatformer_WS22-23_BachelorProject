using System;
using System.Linq;
using Gameplay;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using GlobalSystems;
using UnityEngine;

namespace GameplaySystems
{
    public class BeatManager : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES

        private CharacterInput _characterInput;
        [SerializeField] private AudioSource[] _trackAudioSources;
        
        [SerializeField] private AudioSource _metronomeStrong;
        [SerializeField] private AudioSource _metronomeWeak;
        public TrackData TrackData;

        #endregion

        #region VARIABLES

        private static BeatManager s_Instance;
        public UpdateType UpdateType => UpdateType.Always;

        public static BeatState BeatState;
        
        private int _activeSource;
        private int _nextSource => _activeSource == 0 ? 1 : 0;
        private double _nextTrackTime;
        
        private LoopPoints _loopPoints;

        private double _beatLength;
        private double _nextBeatTime;
        private int _beatTracker;
        
        [SerializeField] private bool _metronomeOn;

        #endregion

        public Action BeatAction;

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

            // TODO: starting value will have to change to BeatState.StandBy in levels and BeatState.Off in menus
            BeatState = BeatState.Active;
            
            _characterInput = in_gameStateManager.CharacterInput;
            
            _beatLength = 60 / (double) TrackData.BPM;
            double barLength = _beatLength * TrackData.Meter;

            _loopPoints.start = barLength * TrackData.IntroBars;
            _loopPoints.end =
                (Mathf.FloorToInt(_trackAudioSources[_activeSource].clip.length / (float)barLength)
                 - TrackData.TailBars) * barLength;
            
            _nextTrackTime = AudioSettings.dspTime + _loopPoints.end - _loopPoints.start;
            _nextBeatTime = AudioSettings.dspTime;
            
            _trackAudioSources[_activeSource].Play();
            _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
        }

        public void CustomUpdate()
        {
            // temp
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            //
            
#if UNITY_EDITOR
            if (_debugBeatOff)
                return;
#endif
            
            double time = AudioSettings.dspTime;
            
            if (time >= _nextTrackTime)
            {
                _activeSource = _nextSource;
                _nextTrackTime += _loopPoints.end - _loopPoints.start;
                _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
            }

            if (time >= _nextBeatTime)
            {
                BeatAction?.Invoke();
                
                _nextBeatTime += _beatLength;
                _beatTracker = _beatTracker < TrackData.Meter ? _beatTracker + 1 : 1;

                if (TrackData.EventBeats.Any(b => b == _beatTracker))
                {
                    if (_metronomeOn)
                        _metronomeStrong.Play();

                    _characterInput.InputState.JumpSquat = true;
                } 
                else if (_metronomeOn)
                    _metronomeWeak.Play();
            }
        }
    }
}
