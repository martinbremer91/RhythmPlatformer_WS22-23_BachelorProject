using System.Collections.Generic;
using System.Linq;
using Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems
{
    public class InputPlaybackManager : GameplayComponent
    {
        #region  REFERENCES

        [SerializeField] private CharacterInput _characterInput;
        [SerializeField] private CharacterStateController _characterStateController;
        [SerializeField] private CharacterMovement _characterMovement;

        [SerializeField] private GameObject playIcon;
        [SerializeField] private GameObject recordIcon;
        
        #endregion

        #region VARIABLES

        public static bool s_PlaybackActive;
        private bool _playbackToggle;
        
        private bool _recording;
        private bool _recordToggle;

        private readonly Queue<InputState> PlaybackQueue = new();

        private Vector3 _playerPosition;
        private MovementSnapshot _movementSnapshot;
        private CharacterState _playerState;

        private DefaultControls _playbackControls;

        #endregion

        private void Awake()
        {
            _playbackControls = CharacterInput.s_Controls;
            
            _playbackControls.Playback.ToggleRecording.performed += _ => HandleRecordingInput();
            _playbackControls.Playback.TogglePlayback.performed += _ => HandlePlaybackInput();
        }

        public override void OnUpdate()
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

                     // TODO: get  current track data
                 }
             }

             if (_recording)
                 RecordInput();

             void RecordInput()
             {
                 InputState inputState = _characterInput.InputState;

                 if (_characterStateController.JumpSquat)
                     inputState.JumpButton = InputActionPhase.Performed;
                 
                 PlaybackQueue.Enqueue(inputState);
             }
         }

         private void HandlePlayback()
         {
             if (_playbackToggle)
             {
                 _playbackToggle = false;
                 
                 if (PlaybackQueue == null || !PlaybackQueue.Any())
                     return;
                 
                 s_PlaybackActive = !s_PlaybackActive;
                 playIcon.SetActive(s_PlaybackActive);

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
                     return;
                 }

                 InputState inputState = PlaybackQueue.Dequeue();

                 if (inputState.JumpButton == InputActionPhase.Performed)
                     _characterStateController.JumpSquat = true;
                 
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
