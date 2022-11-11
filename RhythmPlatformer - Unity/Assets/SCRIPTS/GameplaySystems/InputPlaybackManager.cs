using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Interfaces_and_Enums;
using Structs;
using GlobalSystems;
using UnityEngine;

namespace GameplaySystems
{
    public class InputPlaybackManager : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region  REFERENCES

        private GameStateManager _gameStateManager;
        private CharacterInput _characterInput;
        private CharacterStateController _characterStateController;
        private CharacterMovement _characterMovement;

        private GameObject _playIcon;
        private GameObject _recordIcon;
        
        #endregion

        #region VARIABLES

        public UpdateType UpdateType => UpdateType.Always;
        
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

        public void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            _characterInput = in_gameStateManager.CharacterInput;
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
            
            _playbackControls = UniversalInputManager.s_Controls;

            _playIcon = in_gameStateManager.UiManager.PlayIcon;
            _recordIcon = in_gameStateManager.UiManager.RecordIcon;
            
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
                 _recordIcon.SetActive(_recording);

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
                     _gameStateManager.TogglePhysicsPause(true);
                     return; 
                 }

                _gameStateManager.TogglePhysicsPause(false);
                s_FrameAdvance = false;
             }
             
             if (_playbackToggle)
             {
                 _playbackToggle = false;
                 
                 if (PlaybackQueue == null || !PlaybackQueue.Any())
                     return;
                 
                 _gameStateManager.InputDisabled = !_gameStateManager.InputDisabled;
                 _playIcon.SetActive(_gameStateManager.InputDisabled);
#if UNITY_EDITOR
                 if (!_gameStateManager.InputDisabled)
                 {
                     s_FrameByFrameMode = false;
                     _gameStateManager.TogglePhysicsPause(false);
                 }
#endif
                 _characterStateController.transform.position = _playerPosition;
                 _characterMovement.ApplyMovementSnapshot(_movementSnapshot);
                 _characterStateController.CurrentCharacterState = _playerState;

                 // TODO: sync with track data (skip to starting point or wait)
             }
            
             if (_gameStateManager.InputDisabled)
                 PlaybackInput();

             void PlaybackInput()
             {
                 if (!PlaybackQueue.Any())
                 {
                     _gameStateManager.InputDisabled = false;
                     _playIcon.SetActive(false);
#if UNITY_EDITOR
                     s_FrameByFrameMode = false;
                    _gameStateManager.TogglePhysicsPause(false);
#endif
                     return;
                 }

                 InputState inputState = PlaybackQueue.Dequeue();
                 _characterInput.InputState = inputState;
             }
         }

        private void HandleRecordingInput()
        {
            if (_gameStateManager.InputDisabled)
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
