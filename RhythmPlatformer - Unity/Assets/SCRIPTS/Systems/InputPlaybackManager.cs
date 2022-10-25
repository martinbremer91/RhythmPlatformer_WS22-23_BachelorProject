using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class InputPlaybackManager : MonoBehaviour, IUpdatable
    {
        #region  REFERENCES

        [SerializeField] private CharacterInput _characterInput;
        [SerializeField] private CharacterStateController _characterStateController;
        [SerializeField] private CharacterMovement _characterMovement;

        [SerializeField] private GameObject playIcon;
        [SerializeField] private GameObject recordIcon;
        
        #endregion

        #region VARIABLES

        public UpdateType UpdateType => UpdateType.Always;

        public static bool s_PlaybackActive;
        private bool _playbackToggle;
        
        private bool _recording;
        private bool _recordToggle;

        private readonly Queue<InputState> PlaybackQueue = new();

        private Vector3 _playerPosition;
        private MovementSnapshot _movementSnapshot;
        private CharacterState _playerState;

        private DefaultControls _playbackControls;

#if UNITY_EDITOR
        public static bool s_FrameByFrameMode;
        public static bool s_FrameAdvance;
#endif
        #endregion

        private void OnEnable() => (this as IUpdatable).RegisterUpdatable(true);
        private void OnDisable() => (this as IUpdatable).DeregisterUpdatable(true);

        private void Awake()
        {
            _playbackControls = UniversalInputManager.s_Controls;
            
            _playbackControls.Playback.ToggleRecording.performed += _ => HandleRecordingInput();
            _playbackControls.Playback.TogglePlayback.performed += _ => HandlePlaybackInput();
        }

        public void CustomUpdate()
        {
            HandleRecording();
            HandlePlayback();
        }

         private void HandleRecording()
         {
             if (_recordToggle)
             {
                 _recordToggle = false;
                 _recording = !_recording;
                 recordIcon.SetActive(_recording);

                 if (_recording)
                 {
                     PlaybackQueue.Clear();

                     _playerPosition = _characterStateController.transform.position;
                     _movementSnapshot = _characterMovement.GetMovementSnapshot();
                     _playerState = _characterStateController.CurrentCharacterState;

                     // TODO: get  current track data (pos within current bar)
                 }
             }

             if (_recording)
                 RecordInput();

             void RecordInput()
             {
                 InputState inputState = _characterInput.InputState;
                 PlaybackQueue.Enqueue(inputState);
             }
         }

         private void HandlePlayback()
         {
             if (s_FrameByFrameMode)
             {
                 if (!s_FrameAdvance)
                 {
                     GameStateManager.s_ActiveUpdateType = UpdateType.Paused;
                     return; 
                 }
                 
                GameStateManager.s_ActiveUpdateType = UpdateType.GamePlay;
                s_FrameAdvance = false;
             }
             
             if (_playbackToggle)
             {
                 _playbackToggle = false;
                 
                 if (PlaybackQueue == null || !PlaybackQueue.Any())
                     return;
                 
                 s_PlaybackActive = !s_PlaybackActive;
                 playIcon.SetActive(s_PlaybackActive);
#if UNITY_EDITOR
                 if (!s_PlaybackActive)
                 {
                     s_FrameByFrameMode = false;
                     GameStateManager.s_ActiveUpdateType = UpdateType.GamePlay;
                 }
#endif
                 _characterStateController.transform.position = _playerPosition;
                 _characterMovement.ApplyMovementSnapshot(_movementSnapshot);
                 _characterStateController.CurrentCharacterState = _playerState;

                 // TODO: sync with track data (skip to starting point or wait)
             }
            
             if (s_PlaybackActive)
                 PlaybackInput();

             void PlaybackInput()
             {
                 if (!PlaybackQueue.Any())
                 {
                     s_PlaybackActive = false;
                     playIcon.SetActive(false);
#if UNITY_EDITOR
                     s_FrameByFrameMode = false;
                     GameStateManager.s_ActiveUpdateType = UpdateType.GamePlay;
#endif
                     return;
                 }

                 InputState inputState = PlaybackQueue.Dequeue();
                 _characterInput.InputState = inputState;
             }
         }

        private void HandleRecordingInput()
        {
            if (s_PlaybackActive)
                return;
            
            _recordToggle = true;
        }

        private void HandlePlaybackInput()
        {
            if (_recording)
                return;
            
            _playbackToggle = true;
        }
    }
}
