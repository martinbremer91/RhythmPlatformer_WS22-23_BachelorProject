using System;
using System.Linq;
using System.Threading.Tasks;
using Scriptable_Object_Scripts;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility_Scripts;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region REFERENCES

        [SerializeField] private BeatManager _beatManager;
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
        public bool CeilingHit { get; private set; }
        public bool Airborne => 
            CurrentCharacterState is CharacterState.Rise or CharacterState.Fall or CharacterState.Dash;
        public bool Walled => CurrentCharacterState is CharacterState.WallCling or CharacterState.WallSlide;

        private float _runTurnWindow => _movementConfigs.RunTurnWindow;

        public bool NearWallLeft { get; set; }
        public bool NearWallRight { get; set; }
        
        private bool _canWallCling = true;
        public bool CanWallCling => _canWallCling;

        private float wallClingMaxDuration => _movementConfigs.WallClingMaxDuration;
        public float WallClingTimer {get; private set;}

        #endregion

        #region DELEGATES

        public Action JumpSquatStarted;
        public Action DashWindupStarted;

        #endregion

        #region STATE CHANGE FUNCTIONS
        
        public void SetCharacterState(CharacterState in_value)
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
                case CharacterState.Dash:
                    _characterMovement.DashCurveTracker.x = 0;
                    _characterMovement.DashDirection = Vector2.zero;
                    _characterMovement.DashVelocity = Vector2.zero;
                    break;
            }
        }
        
        #endregion

        #region INIT & UPDATE

        private void Awake()
        {
            GetAnticipationStatesDurations();
            _beatManager.BeatEvent += PerformJumpSquatAsync;
        }

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
                    if (!in_enter && CurrentCharacterState != CharacterState.Dash)
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
                    CeilingHit = in_enter;
                    if (in_enter)
                    {
                        CurrentCharacterState = CharacterState.Fall;
                        _characterMovement.CancelVerticalVelocity();
                    }
                    break;
                case CollisionCheck.LeftWall:
                    NearWallLeft = in_enter;
                    if (in_enter && _characterMovement.CharacterVelocity.x < 0)
                    {
                        SetWalledState(false);
                        _characterMovement.CancelHorizontalVelocity();
                    }
                    break;
                case CollisionCheck.RightWall:
                    NearWallRight = in_enter;
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
                        bool holdTowardsWall_L = NearWallLeft && inputX < -.5f;
                        bool holdTowardsWall_R = NearWallRight && inputX > .5f;
                        
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
                case CharacterState.Dash:
                    if (_characterMovement.DashCurveTracker.x >= _characterMovement.DashCurveTracker.y)
                        CurrentCharacterState = CharacterState.Fall;
                    break;
            }
            
            if (NearWallLeft)
                SetWalledState(false);
            else if (NearWallRight)
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
                {
                    if (_characterMovement.CharacterVelocity.x * _characterInput.InputState.DirectionalInput.x < 0)
                        CheckFacingOrientation();
                    
                    CurrentCharacterState =
                        Mathf.Abs(_characterInput.InputState.DirectionalInput.x) > .5f
                            ? CharacterState.Run
                            : CharacterState.Idle;
                }
                else
                    CurrentCharacterState = CharacterState.Crouch;
            }

            void HandleWalled()
            {
                if (!NearWallLeft && !NearWallRight)
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
                        _characterMovement.RunCurveTracker.x += Time.deltaTime;
                    _characterMovement.Run();
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
                case CharacterState.Dash:
                    Vector2 dashTracker = _characterMovement.DashCurveTracker;
                    if (dashTracker.x < dashTracker.y)
                    {
                        _characterMovement.DashCurveTracker.x += Time.deltaTime;
                        _characterMovement.Dash();
                    }
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
        
        /// <summary>
        /// Takes care of FacingLeft property only. Can take Walled state in to account by passing in in_walled param.
        /// </summary>
        /// <param name="in_walled"></param>
        /// <param name="in_slide"></param>
        public void CheckFacingOrientation(bool in_walled = false, bool in_slide = false)
        {
            float turnParam = 
                in_slide ? _characterMovement.CharacterVelocity.x : _characterInput.InputState.DirectionalInput.x;
            
            if (FacingLeft == !in_walled && turnParam > 0)
                FacingLeft = in_walled;
            if (FacingLeft == in_walled && turnParam < 0)
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

        private void PerformJumpSquatAsync()
        {
            // JumpSquatStarted?.Invoke();
            //
            // float timer = _jumpSquatDuration;
            //
            // while (timer > 0)
            // {
            //     await Task.Yield();
            //     timer -= Time.deltaTime;
            //     
            //     if (DashWindup)
            //     {
            //         // TODO: call DashCanceledJump delegate (to trigger VFX feedback)
            //         JumpSquat = false;
            //         return;
            //     }
            // }

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
            CheckFacingOrientation();
            _characterMovement.InitializeDash();
        }
        
        private void SetWalledState(bool in_RightWall)
        {
            bool leftWall = !in_RightWall || NearWallLeft;
            bool rightWall = in_RightWall || NearWallRight;
            
            float inputX = _characterInput.InputState.DirectionalInput.x;
            float velocityX = _characterMovement.CharacterVelocity.x;

            bool holdTowardsWall_L = leftWall && inputX < -.5f && velocityX <= 0;
            bool holdTowardsWall_R = rightWall && inputX > .5f && velocityX >= 0;

            if (CanWallCling && Airborne && (holdTowardsWall_L || holdTowardsWall_R))
                CurrentCharacterState = CharacterState.WallSlide;
            else if (CurrentCharacterState == CharacterState.Dash)
                CurrentCharacterState = CharacterState.Fall;

            if (CurrentCharacterState == CharacterState.WallSlide && 
                _characterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
                CurrentCharacterState = CharacterState.Fall;
        }
        
        #endregion
    }
}
