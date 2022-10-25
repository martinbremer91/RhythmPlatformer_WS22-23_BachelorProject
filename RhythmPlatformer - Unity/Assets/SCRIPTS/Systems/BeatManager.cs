using System.Linq;
using Gameplay;
using Interfaces;
using Scriptable_Object_Scripts;
using UnityEngine;

namespace Systems
{
    public class BeatManager : GameplayComponent, IInit<GameStateManager>
    {
        private static BeatManager s_Instance;
        
        #region REFERENCES

        private CharacterInput _characterInput;
        [SerializeField] private AudioSource[] _trackAudioSources;

        #endregion
        
        private int _activeSource;
        private int _nextSource => _activeSource == 0 ? 1 : 0;
        private double _nextTrackTime;
        
        public TrackData TrackData;
        private LoopPoints _loopPoints;

        [SerializeField] private AudioSource _metronomeStrong;
        [SerializeField] private AudioSource _metronomeWeak;

        private double _beatLength;
        private double _nextBeatTime;
        private int _beatTracker;
        
        [SerializeField] private bool _metronomeOn;

#if UNITY_EDITOR
        public int ActiveSource => _activeSource;
        public int BeatTracker => _beatTracker;
#endif
        private struct LoopPoints
        {
            public double start;
            public double end;
        }

        public void Init(GameStateManager in_gameStateManager)
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

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

        public override void CustomUpdate()
        {
            // temp
            if (InputPlaybackManager.s_PlaybackActive)
                return;
            //
            
            double time = AudioSettings.dspTime;
            
            if (time >= _nextTrackTime)
            {
                _activeSource = _nextSource;
                _nextTrackTime += _loopPoints.end - _loopPoints.start;
                _trackAudioSources[_nextSource].PlayScheduled(_nextTrackTime);
            }

            if (time >= _nextBeatTime)
            {
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

    public enum BeatState
    {
        Off,
        Active,
        Standby
    }
}
