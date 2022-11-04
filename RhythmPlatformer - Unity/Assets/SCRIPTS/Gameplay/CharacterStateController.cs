using System;
using System.Linq;
using System.Threading.Tasks;
using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using GlobalSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility_Scripts;

namespace Gameplay
{
    public class CharacterStateController : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES
        
        private CharacterSpriteController _spriteController;
        private CharacterMovement _characterMovement;
        private CharacterInput _characterInput;
        private MovementConfigs _movementConfigs;

        #endregion
        
        #region VARIABLES
        
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        private CharacterState _currentCharacterState;
        public CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            set => SetCharacterState(value);
        }

        private bool m_jumpSquat;
        private bool _jumpSquat
        {
            get => m_jumpSquat;
            set
            {
                if (m_jumpSquat == value)
                    return;
                m_jumpSquat = value;
                if (value)
                    PerformJumpSquatAsync();
                else
                    _characterInput.InputState.JumpSquat = false;
            }
        }
        private float _jumpSquatDuration;

        private bool _dashWindup;
        public bool DashWindup
        {
            get => _dashWindup;
            private set
            {
                if (_dashWindup == value)
                    return;
                _dashWindup = value;
                if (value)
                    PerformDashWindupAsync();
                else
                    _characterInput.InputState.DashWindup = false;
            }
        }
        private float _dashWindupDuration;

        private bool _canDash;

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

        public int LookAheadDirection;
        
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

        #region STATE CHANGE FUNCTIONS
        
        private void SetCharacterState(CharacterState in_value)
        {
            if (_currentCharacterState != CharacterState.Rise && _currentCharacterState == in_value)
                return;
            
            ChangeIntoState(in_value);
            ChangeOutOfState();

            _currentCharacterState = in_value;
            _spriteController.HandleStateAnimation();
        }

        private void ChangeIntoState(CharacterState in_value)
        {
            switch (in_value)
            {
                case CharacterState.Run:
                    CheckFacingOrientation();
                    break;
                case CharacterState.Fall:
                    _characterMovement.FastFalling = false;
                    _characterMovement.YAxisReadyForFastFall = _characterInput.InputState.DirectionalInput.y >= -.5f;
                    _characterMovement.FallCurveTracker.x = 0;
                    _characterMovement.FallVelocity = new(_characterMovement.CharacterVelocity.x, 0);
                    break;
                case CharacterState.Rise:
                    _characterMovement.RiseCurveTracker.x = 0;
                    _characterMovement.InitializeRise();
                    _canDash = true;
                    break;
                case CharacterState.Land:
                    _characterMovement.LandVelocity = _characterMovement.CharacterVelocity.x;
                    break;
                case CharacterState.WallCling:
                    _characterMovement.CancelVerticalVelocity();
                    CheckFacingOrientation(true);
                    break;
                case CharacterState.WallSlide:
                    CheckFacingOrientation(true);
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
                    _characterMovement.FastFalling = false;
                    _characterMovement.YAxisReadyForFastFall = false;
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

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterInput = in_gameStateManager.CharacterInput;
            _characterMovement = in_gameStateManager.CharacterMovement;
            _movementConfigs = in_gameStateManager.MovementConfigs;
            _spriteController = in_gameStateManager.CharacterSpriteController;
            
            GetAnticipationStatesDurations();
            
            void GetAnticipationStatesDurations()
            {
                AnimationClip dashWindupClip =
                    _spriteController.PlayerAnimator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(c => c.name == Constants.DashWindupClipName);
                
                if (dashWindupClip == null)
                    throw new Exception("Could not find AnimationClip with name " + Constants.DashWindupClipName);
                
                _dashWindupDuration = dashWindupClip.length;
            }
        } 
        
        public void CustomUpdate()
        {
            if (_characterInput.InputState.JumpSquat)
                _jumpSquat = true;
            if (_characterInput.InputState.DashWindup)
                DashWindup = true;
            
            CheckCharacterLookingAhead();
            HandleInputStateChange();
            ApplyStateMovement();
        }
        
        #endregion

        #region STATE PROCESSING FUNCTIONS

        private void CheckCharacterLookingAhead()
        {
            float inputDirectionY = _characterInput.InputState.DirectionalInput.y;
            LookAheadDirection = !Grounded ? 0 :
                inputDirectionY > .38f ? 1 :
                inputDirectionY < -.38f ? -1 : 0;
        }
        
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
                    if (in_enter && _characterMovement.CharacterVelocity.x < 0)
                    {
                        HandleWallInteractions(false);
                        _characterMovement.CancelHorizontalVelocity();
                    }
                    break;
                case CollisionCheck.RightWall:
                    if (in_enter && _characterMovement.CharacterVelocity.x > 0)
                    {
                        HandleWallInteractions(true);
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
                    if (CanWallCling && _characterInput.InputState.WallClingTrigger == InputActionPhase.Performed &&
                        Mathf.Abs(_characterMovement.CharacterVelocity.y) <= 1)
                        CurrentCharacterState = CharacterState.WallCling;
                    break;
                case CharacterState.Dash:
                    if (_characterMovement.DashCurveTracker.x >= _characterMovement.DashCurveTracker.y)
                        CurrentCharacterState = CharacterState.Fall;
                    break;
            }
            
            if (NearWallLeft)
                HandleWallInteractions(false);
            else if (NearWallRight)
                HandleWallInteractions(true);

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
                        _characterMovement.RunCurveTracker.x += Time.fixedDeltaTime;
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
                        _characterMovement.RiseCurveTracker.x += Time.fixedDeltaTime * (1 / _characterMovement.RiseSpeedMod);
                        _characterMovement.Rise();
                    }
                    else
                        CurrentCharacterState = CharacterState.Fall;
                    break;
                case CharacterState.Fall:
                    Vector2 fallTracker = _characterMovement.FallCurveTracker;
                    if (fallTracker.x < fallTracker.y)
                    {
                        _characterMovement.FallCurveTracker.x += Time.fixedDeltaTime;
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
                        _characterMovement.DashCurveTracker.x += Time.fixedDeltaTime;
                        _characterMovement.Dash();
                    }
                    break;
            }

            void DecrementWallClingTimer()
            {
                WallClingTimer = Mathf.Max(WallClingTimer - Time.fixedDeltaTime, 0);
                if (WallClingTimer <= 0)
                    _canWallCling = true;
            }
        }
        
        public void IncrementWallClingTimer()
        {
            WallClingTimer = Mathf.Min(WallClingTimer + Time.fixedDeltaTime, wallClingMaxDuration);
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
        /// <param name="in_slideOrDash"></param>
        public void CheckFacingOrientation(bool in_walled = false, bool in_slideOrDash = false)
        {
            float turnParam = 
                in_slideOrDash ? _characterMovement.CharacterVelocity.x : _characterInput.InputState.DirectionalInput.x;
            
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
                runTimer -= Time.fixedDeltaTime;
                
                if (CurrentCharacterState == CharacterState.Run)
                    return;
            }
            
            _characterMovement.RunCurveTracker.x = 0;
        }

        private void PerformJumpSquatAsync()
        {
            // TODO: change jump squat to happen at t - jumpSquatDuration
            
            // JumpSquatStarted?.Invoke();
            //
            // float timer = _jumpSquatDuration;
            //
            // while (timer > 0)
            // {
            //     await Task.Yield();
            //     timer -= Time.fixedDeltaTime;
            //     
            //     if (DashWindup)
            //     {
            //         // TODO: call DashCanceledJump delegate (to trigger VFX feedback)
            //         _jumpSquat = false;
            //         return;
            //     }
            // }

            _jumpSquat = false;
            CurrentCharacterState = CharacterState.Rise;
        }

        private async void PerformDashWindupAsync()
        {
            if (!_canDash)
            {
                _dashWindup = false;
                return;
            }

            _canDash = false;
            _spriteController.SetDashWindupTrigger();
            
            float timer = _dashWindupDuration;

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.fixedDeltaTime;
            }

            DashWindup = false;
            CheckFacingOrientation();
            _characterMovement.InitializeDash();
        }
        
        private void HandleWallInteractions(bool in_RightWall)
        {
            bool leftWall = !in_RightWall || NearWallLeft;
            bool rightWall = in_RightWall || NearWallRight;
            
            float inputX = _characterInput.InputState.DirectionalInput.x;
            float velocityX = _characterMovement.CharacterVelocity.x;

            bool holdTowardsWall_L = leftWall && inputX < -.5f && velocityX <= 0;
            bool holdTowardsWall_R = rightWall && inputX > .5f && velocityX >= 0;

            if (CanWallCling && Airborne && (holdTowardsWall_L || holdTowardsWall_R))
                CurrentCharacterState = CharacterState.WallSlide;

            if (CurrentCharacterState == CharacterState.WallSlide && 
                _characterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
                CurrentCharacterState = CharacterState.Fall;

            if (CanWallCling && _characterInput.InputState.WallClingTrigger == InputActionPhase.Performed)
            {
                if (Mathf.Abs(_characterMovement.CharacterVelocity.y) <= 1)
                    CurrentCharacterState = CharacterState.WallCling;
                else
                    CurrentCharacterState = CharacterState.WallSlide;
            }
        }

        #endregion
    }
}
