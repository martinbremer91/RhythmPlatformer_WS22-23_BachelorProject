using System;
using System.Linq;
using System.Threading.Tasks;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region REFERENCES

        [SerializeField] private CharacterSpriteController _spriteController;
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private MovementConfigs _movementConfigs;
        [SerializeField] private CharacterInput _characterInput;

        #endregion
        
        #region VARIABLES

        private CharacterState _currentCharacterState;
        public CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            private set => SetCharacterState(value);
        }

        private bool _jumpSquat;
        public bool JumpSquat
        {
            get => _jumpSquat;
            set
            {
                if (_jumpSquat == value)
                    return;
                _jumpSquat = value;
                if (value)
                    PerformJumpSquatAsync();
            }
        }
        private float _jumpSquatDuration;

        private bool _dashWindup;
        public bool DashWindup
        {
            get => _dashWindup;
            set
            {
                if (_dashWindup == value)
                    return;
                _dashWindup = value;
                if (value)
                    PerformDashWindupAsync();
            }
        }
        private float _dashWindupDuration;
        
        private bool _dashing;

        private bool _facingLeft;
        public bool FacingLeft
        {
            get => _facingLeft;
            private set
            {
                if (_facingLeft == value)
                    return;
                
                _spriteController.SetCharacterOrientation(value);
                _facingLeft = value;
            }
        }
        
        public bool Grounded =>
            CurrentCharacterState is CharacterState.Idle or CharacterState.Run or CharacterState.Land
                or CharacterState.Crouch;
        
        public bool Airborne => CurrentCharacterState is CharacterState.Rise or CharacterState.Fall;
        public bool Walled => CurrentCharacterState is CharacterState.WallCling or CharacterState.WallSlide;

        private float _runTurnWindow => _movementConfigs.RunTurnWindow;

        public bool NearWall_L { get; set; }
        public bool NearWall_R { get; set; }
        
        private bool _canWallCling = true; 
#if UNITY_EDITOR
        public bool CanWallCling => _canWallCling;
#endif
        
        private float wallClingMaxDuration => _movementConfigs.WallClingMaxDuration;
        public float WallClingTimer {get; private set;}

        #endregion

        #region DELEGATES

        public Action JumpSquatStarted;
        public Action DashWindupStarted;

        #endregion

        #region STATE CHANGE FUNCTIONS

        private void SetCharacterState(CharacterState in_value)
        {
            if (_currentCharacterState != CharacterState.Rise && _currentCharacterState == in_value)
                return;
            
            ChangeIntoState(in_value);
            ChangeOutOfState();
            
            _currentCharacterState = in_value;
        }

        private void ChangeIntoState(CharacterState in_value)
        {
            switch (in_value)
            {
                case CharacterState.Run:
                    CheckFacingOrientation();
                    break;
                case CharacterState.Fall:
                    _characterMovement.FallCurveTracker.x = 0;
                    _characterMovement.FallVelocity = new(_characterMovement.CharacterVelocity.x, 0);
                    break;
                case CharacterState.Rise:
                    _characterMovement.RiseCurveTracker.x = 0;
                    _characterMovement.InitializeRise();
                    break;
                case CharacterState.Land:
                    _characterMovement.LandVelocity = _characterMovement.CharacterVelocity.x;
                    break;
                case CharacterState.WallCling:
                    CheckFacingOrientation(true);
                    break;
                case CharacterState.WallSlide:
                    CheckFacingOrientation(true, true);
                    _characterMovement.FallCurveTracker.x = 0;
                    _characterMovement.WallSlideVelocity = _characterMovement.CharacterVelocity.y;
                    break;
            }
        }

        private void ChangeOutOfState()
        {
            switch (_currentCharacterState)
            {
                case CharacterState.Run:
                    ResetRunCurveTrackerAsync();
                    _characterMovement.RunVelocity = 0;
                    break;
                case CharacterState.Land:
                    _characterMovement.LandVelocity = 0;
                    break;
                case CharacterState.Rise:
                    _characterMovement.RiseVelocity = Vector2.zero;
                    break;
                case CharacterState.Fall:
                    _characterMovement.FallVelocity = Vector2.zero;
                    break;
                case CharacterState.WallSlide:
                    _characterMovement.WallSlideVelocity = 0;
                    break;
            }
        }
        
        #endregion

        #region INIT & UPDATE

        private void Awake() => GetAnticipationStatesDurations();

        private void GetAnticipationStatesDurations()
        {
            AnimationClip jumpSquatClip =
                _spriteController.PlayerAnimator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name == Constants.JumpSquatClipName);
            AnimationClip dashWindupClip =
                _spriteController.PlayerAnimator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name == Constants.DashWindupClipName);

            if (jumpSquatClip == null)
                throw new Exception("Could not find AnimationClip with name " + Constants.JumpSquatClipName);
            if (dashWindupClip == null)
                throw new Exception("Could not find AnimationClip with name " + Constants.JumpSquatClipName);

            _jumpSquatDuration = jumpSquatClip.length;
            _dashWindupDuration = dashWindupClip.length;
        }

        public override void OnUpdate()
        { 
            HandleInputStateChange();
            ApplyStateMovement();
        }
        
        #endregion

        #region STATE PROCESSING FUNCTIONS
        
        public void HandleCollisionStateChange(CollisionCheck in_check, bool in_enter)
        {
            switch (in_check)
            {
                case CollisionCheck.Ground:
                    if (!in_enter)
                        CurrentCharacterState = _characterMovement.CharacterVelocity.y > 0
                            ? CharacterState.Rise
                            : CharacterState.Fall;
                    else
                    {
                        CurrentCharacterState = CharacterState.Land;
                        _characterMovement.CancelVerticalVelocity();
                    }
                    break;
                case CollisionCheck.Ceiling:
                    if (in_enter)
                    {
                        CurrentCharacterState = CharacterState.Fall;
                        _characterMovement.CancelVerticalVelocity();
                    }
                    break;
                case CollisionCheck.LWall:
                    NearWall_L = in_enter;
                    if (in_enter && _characterMovement.CharacterVelocity.x < 0)
                    {
                        SetWalledState(false);
                        _characterMovement.CancelHorizontalVelocity();
                    }
                    break;
                case CollisionCheck.RWall:
                    NearWall_R = in_enter;
                    if (in_enter && _characterMovement.CharacterVelocity.x > 0)
                    {
                        SetWalledState(true);
                        _characterMovement.CancelHorizontalVelocity();
                    }
                    break;
            }
        }

        private void HandleInputStateChange()
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Idle:
                    HandleIdle();
                    break;
                case CharacterState.Run:
                    HandleRun();
                    break;
                case CharacterState.Crouch:
                    if (_characterInput.InputState.DirectionalInput.y > -.5f)
                        CurrentCharacterState = 
                            Mathf.Abs(_characterInput.InputState.DirectionalInput.x) > .5f ? 
                                CharacterState.Run : CharacterState.Idle;
                    break;
                case CharacterState.Land:
                    if (_characterMovement.CharacterVelocity.x * _characterInput.InputState.DirectionalInput.x > 0 &&
                        Mathf.Abs(_characterMovement.CharacterVelocity.x) <= _characterMovement.RunTopSpeed)
                    {
                        CurrentCharacterState = CharacterState.Run;
                        _characterMovement.RunVelocity = _characterMovement.CharacterVelocity.x;
                        break;
                    }
                    if (_characterMovement.CharacterVelocity.x == 0)
                        CurrentCharacterState = 
                            _characterInput.InputState.DirectionalInput.y <= -.5f ? 
                                CharacterState.Crouch : CharacterState.Idle;
                    break;
                case CharacterState.WallCling:
                    HandleWalled();
                    if (_characterInput.InputState.WallClingTrigger != InputActionPhase.Performed)
                    {
                        float inputX = _characterInput.InputState.DirectionalInput.x;
                        bool holdTowardsWall_L = NearWall_L && inputX < -.5f;
                        bool holdTowardsWall_R = NearWall_R && inputX > .5f;
                        
                        CurrentCharacterState = holdTowardsWall_L || holdTowardsWall_R ? 
                            CharacterState.WallSlide : CharacterState.Fall;
                    }
                    break;
                case CharacterState.WallSlide:
                    HandleWalled();
                    if (Mathf.Abs(_characterMovement.CharacterVelocity.y) <= .1f &&
                        _characterInput.InputState.WallClingTrigger == InputActionPhase.Performed)
                        CurrentCharacterState = CharacterState.WallCling;
                    break;
            }
            
            if (NearWall_L)
                SetWalledState(false);
            else if (NearWall_R)
                SetWalledState(true);

            void HandleIdle()
            {
                if (_characterInput.InputState.DirectionalInput.y < -.5f)
                {
                    CurrentCharacterState = CharacterState.Crouch;
                    return;
                }

                if (Mathf.Abs(_characterInput.InputState.DirectionalInput.x) > .5f)
                {
                    CurrentCharacterState = CharacterState.Run;
                    return;
                }
                
                CheckFacingOrientation();
            }

            void HandleRun()
            {
                if (_characterInput.InputState.DirectionalInput.y > -.5f)
                    CurrentCharacterState =
                        Mathf.Abs(_characterInput.InputState.DirectionalInput.x) > .5f
                            ? CharacterState.Run
                            : CharacterState.Idle;
                else
                    CurrentCharacterState = CharacterState.Crouch;
            }

            void HandleWalled()
            {
                if (!NearWall_L && !NearWall_R)
                    CurrentCharacterState = CharacterState.Fall;
            }
        }
        
        private void ApplyStateMovement()
        {
            if (!Walled)
                DecrementWallClingTimer();
            
            switch (CurrentCharacterState)
            {
                case CharacterState.Idle:
                    break;
                case CharacterState.Run:
                    Vector2 runTracker = _characterMovement.RunCurveTracker;
                    if (runTracker.x < runTracker.y || _characterMovement.RunVelocity == 0)
                    {
                        _characterMovement.RunCurveTracker.x += Time.deltaTime;
                        _characterMovement.Run();
                    }
                    break;
                case CharacterState.Crouch:
                    break;
                case CharacterState.Land:
                    _characterMovement.Land();
                    break;
                case CharacterState.Rise:
                    Vector2 riseTracker = _characterMovement.RiseCurveTracker;
                    if (riseTracker.x < riseTracker.y)
                    {
                        _characterMovement.RiseCurveTracker.x += Time.deltaTime * (1 / _characterMovement.RiseSpeedMod);
                        _characterMovement.Rise();
                    }
                    else
                        CurrentCharacterState = CharacterState.Fall;
                    break;
                case CharacterState.Fall:
                    Vector2 fallTracker = _characterMovement.FallCurveTracker;
                    if (fallTracker.x < fallTracker.y)
                    {
                        _characterMovement.FallCurveTracker.x += Time.deltaTime;
                        _characterMovement.Fall();
                    }
                    break;
                case CharacterState.WallCling:
                    IncrementWallClingTimer();
                    break;
                case CharacterState.WallSlide:
                    _characterMovement.WallSlide();
                    break;
            }

            void DecrementWallClingTimer()
            {
                WallClingTimer = Mathf.Max(WallClingTimer - Time.deltaTime, 0);
                if (WallClingTimer <= 0)
                    _canWallCling = true;
            }
        }
        
        public void IncrementWallClingTimer()
        {
            WallClingTimer = Mathf.Min(WallClingTimer + Time.deltaTime, wallClingMaxDuration);
            if (WallClingTimer >= wallClingMaxDuration)
            {
                _canWallCling = false;
                CurrentCharacterState = CharacterState.Fall;
            }
        }
        
        #endregion

        #region UTILITY FUNCTIONS
        
        private void CheckFacingOrientation(bool in_walled = false, bool in_slide = false)
        {
            float turnParam = 
                in_slide ? _characterMovement.CharacterVelocity.x : _characterInput.InputState.DirectionalInput.x;
            
            if (FacingLeft == !in_walled && turnParam > .1f)
                FacingLeft = in_walled;
            if (FacingLeft == in_walled && turnParam < -.1f)
                FacingLeft = !in_walled;
        }

        /// <summary>
        /// Async function to create time-frame for reversing run direction without losing speed ("dash dancing")
        /// </summary>
        private async void ResetRunCurveTrackerAsync()
        {
            float runTimer = _runTurnWindow;

            while (runTimer > 0)
            {
                await Task.Yield();
                runTimer -= Time.deltaTime;
                
                if (CurrentCharacterState == CharacterState.Run)
                    return;
            }
            
            _characterMovement.RunCurveTracker.x = 0;
        }

        private async void PerformJumpSquatAsync()
        {
            JumpSquatStarted?.Invoke();
            
            float timer = _jumpSquatDuration;

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.deltaTime;
                
                if (DashWindup)
                {
                    JumpSquat = false;
                    return;
                }
            }

            JumpSquat = false;
            CurrentCharacterState = CharacterState.Rise;
        }

        private async void PerformDashWindupAsync()
        {
            DashWindupStarted?.Invoke();
            
            float timer = _dashWindupDuration;

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.deltaTime;
            }

            DashWindup = false;
            // TODO: call Dash logic
        }

        /// <summary>
        /// Checks y velocity and input direction. Sets state to WallCling, WallSlide, or does nothing.
        /// </summary>
        private void SetWalledState(bool in_RightWall)
        {
            bool leftWall = !in_RightWall || NearWall_L;
            bool rightWall = in_RightWall || NearWall_R;
            
            float inputX = _characterInput.InputState.DirectionalInput.x;
            float velocityX = _characterMovement.CharacterVelocity.x;
                
            bool holdTowardsWall_L = leftWall && inputX < -.5f && velocityX <= 0;
            bool holdTowardsWall_R = rightWall && inputX > .5f && velocityX >= 0;

            if (CanWallCling && Airborne && (holdTowardsWall_L || holdTowardsWall_R))
                CurrentCharacterState = CharacterState.WallSlide;

            if (CurrentCharacterState == CharacterState.WallSlide && 
                _characterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
                CurrentCharacterState = CharacterState.Fall;
        }
        
        #endregion
    }
}
