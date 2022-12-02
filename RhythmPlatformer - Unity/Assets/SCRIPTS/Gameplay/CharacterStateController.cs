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

        private GameStateManager _gameStateManager;
        private CharacterCollisionDetector _characterCollisionDetector;
        private CharacterSpriteController _spriteController;
        private CharacterMovement _characterMovement;
        private CharacterInput _characterInput;

        #endregion
        
        #region VARIABLES
        
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        private CharacterState _currentCharacterState;
        public CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            set => SetCharacterState(value);
        }

        private bool m_jumpCommand;
        private bool _jumpCommand
        {
            get => m_jumpCommand;
            set
            {
                if (m_jumpCommand == value)
                    return;
                m_jumpCommand = value;
                if (value)
                    ExecuteJump();
                else
                    _characterInput.InputState.JumpCommand = false;
            }
        }

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
        public bool CanDash {
            get => _canDash;
            set {
                if (value == _canDash)
                    return;

                _canDash = value;
                _spriteController.UpdateCanDashColor(value);
            }
        }

        private bool _facingLeft;
        public bool FacingLeft
        {
            get => _facingLeft;
            set
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

        private float _runTurnWindow;

        public bool NearWallLeft { get; set; }
        public bool NearWallRight { get; set; }
        
        private bool _canWallCling = true;
        public bool CanWallCling => _canWallCling;

        private float _wallClingMaxDuration;
        public float WallClingTimer {get; private set;}

        private float _maxInheritedXVelocity;

        [HideInInspector] public bool Dead;
        
        public Action Respawn;
        public Action BecomeGrounded;
        
#if UNITY_EDITOR
        public static bool s_Invulnerable;
#endif
        
        #endregion

        #region STATE CHANGE FUNCTIONS
        
        private void SetCharacterState(CharacterState in_value)
        {
            if (_currentCharacterState != CharacterState.Rise && _currentCharacterState == in_value)
                return;
            if (in_value is CharacterState.WallCling or CharacterState.WallSlide && CheckIfFastFalling())
                return;

            ChangeIntoState(in_value);
            ChangeOutOfState();

            _currentCharacterState = in_value;
            
            if (!DashWindup)
                _spriteController.HandleStateAnimation();

            if (Grounded)
                BecomeGrounded?.Invoke();

            bool CheckIfFastFalling() =>
                _characterMovement.FastFalling && _characterInput.InputState.DirectionalInput.y <= -.5f;
        }

        private void ChangeIntoState(CharacterState in_value)
        {
            switch (in_value)
            {
                case CharacterState.Run:
                    CanDash = true;
                    CheckFacingOrientation();
                    break;
                case CharacterState.Fall:
                    _characterMovement.FastFalling = false;
                    _characterMovement.YAxisReadyForFastFall = _characterInput.InputState.DirectionalInput.y >= -.5f;
                    _characterMovement.FallCurveTracker.x = 0;
                    float clampedInheritedXVelocityFall = GetClampedInheritedXVelocity();
                    _characterMovement.FallVelocity = new(clampedInheritedXVelocityFall, 0);
                    break;
                case CharacterState.Rise:
                    _characterMovement.RiseCurveTracker.x = 0;
                    _characterMovement.InitializeRise(GetClampedInheritedXVelocity());
                    break;
                case CharacterState.Land:
                    CanDash = true;
                    _characterMovement.LandVelocity = _characterMovement.CharacterVelocity.x;
                    break;
                case CharacterState.WallCling:
                    _characterMovement.CancelVerticalVelocity();
                    CheckFacingOrientation(true);
                    CanDash = true;
                    break;
                case CharacterState.WallSlide:
                    CheckFacingOrientation(true);
                    _characterMovement.FallCurveTracker.x = 0;
                    _characterMovement.WallSlideVelocity = _characterMovement.CharacterVelocity.y;
                    break;
            }

            float GetClampedInheritedXVelocity() {
                return Mathf.Abs(_characterMovement.CharacterVelocity.x) <= _maxInheritedXVelocity ?
                        _characterMovement.CharacterVelocity.x : _characterMovement.CharacterVelocity.x > 0 ?
                        _maxInheritedXVelocity : -_maxInheritedXVelocity;
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
            _gameStateManager = in_gameStateManager;
            _characterCollisionDetector = in_gameStateManager.CharacterCollisionDetector;
            _characterInput = in_gameStateManager.CharacterInput;
            _characterMovement = in_gameStateManager.CharacterMovement;
            _spriteController = in_gameStateManager.CharacterSpriteController;
            
            MovementConfigs movementConfigs = in_gameStateManager.MovementConfigs;
            _maxInheritedXVelocity = movementConfigs.MaxInheritedXVelocity;
            _runTurnWindow = movementConfigs.RunTurnWindow;
            _wallClingMaxDuration = movementConfigs.WallClingMaxDuration;
            
            GetAnticipationStatesDurations();
            
            void GetAnticipationStatesDurations()
            {
                AnimationClip dashWindupClip =
                    _spriteController.Animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(c => c.name == Constants.DashWindupClipName);
                
                if (dashWindupClip == null)
                    throw new Exception("Could not find AnimationClip with name " + Constants.DashWindupClipName);
                
                _dashWindupDuration = dashWindupClip.length;
            }
        }
        
        public void CustomUpdate()
        {
            if (_characterInput.InputState.JumpCommand)
                _jumpCommand = true;
            if (_characterInput.InputState.DashWindup)
                DashWindup = true;
            
            HandleInputStateChange();
            ApplyStateMovement();
        }
        
        #endregion

        #region STATE PROCESSING FUNCTIONS

        public async void DieAsync()
        {
#if UNITY_EDITOR
            if (s_Invulnerable)
                return;
#endif
            Dead = true;
            UiManager uiManager = _gameStateManager.UiManager;
            UniversalInputManager.SetUniversalControlsActive(false);
            _characterInput.SetCharacterControlsActive(false);
            _characterInput.SetInputStateToNeutral();
            
            await uiManager.FadeDarkScreen(true);

            Respawn?.Invoke();
            CurrentCharacterState = CharacterState.Idle;

            await uiManager.FadeDarkScreen(false);
            
            _characterInput.SetCharacterControlsActive(true);
            UniversalInputManager.SetUniversalControlsActive(true);
            Dead = false;
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
                        CurrentCharacterState = CharacterState.Land;
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
                    if (!_characterCollisionDetector.SlideOnHorizontal && _characterMovement.CharacterVelocity.x == 0)
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
                
                if (_characterCollisionDetector.SlideOnHorizontal) {
                    CurrentCharacterState = CharacterState.Land;
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
                        _characterMovement.FallCurveTracker.x += Time.fixedDeltaTime;
                    _characterMovement.Fall();
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
            WallClingTimer = Mathf.Min(WallClingTimer + Time.fixedDeltaTime, _wallClingMaxDuration);
            if (WallClingTimer >= _wallClingMaxDuration)
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

            if (in_walled)
            {
                FacingLeft = NearWallRight;
                return;
            }

            FacingLeft = turnParam > 0 ? false : turnParam < 0 ? true : FacingLeft;
        }

        /// <summary>
        /// Async function to create time-frame for reversing run direction without losing speed ("dash dancing")
        /// </summary>
        private async void ResetRunCurveTrackerAsync()
        {
            float runTimer = _runTurnWindow;

            while (runTimer > 0 && Time.deltaTime > 0)
            {
                await Task.Yield();
                runTimer -= Time.fixedDeltaTime;
                
                if (CurrentCharacterState == CharacterState.Run)
                    return;
            }

            if (Time.deltaTime <= 0)
                return;
            
            _characterMovement.RunCurveTracker.x = 0;
        }

        private void ExecuteJump()
        {
            _jumpCommand = false;
            CurrentCharacterState = CharacterState.Rise;
        }

        private async void PerformDashWindupAsync()
        {
            if (!CanDash)
            {
                DashWindup = false;
                return;
            }

            CanDash = false;
            _spriteController.SetDashWindupTrigger();
            
            float timer = _dashWindupDuration;

            while (timer > 0 && Time.deltaTime > 0)
            {
                await Task.Yield();
                timer -= Time.fixedDeltaTime;
            }

            if (Time.deltaTime <= 0)
                return;

            DashWindup = false;
            CheckFacingOrientation();
            _characterMovement.InitializeDash();
        }
        
        private void HandleWallInteractions(bool in_RightWall)
        {
            if (Grounded)
                return;

            bool leftWall = !in_RightWall || NearWallLeft;
            bool rightWall = in_RightWall || NearWallRight;
            
            float inputX = _characterInput.InputState.DirectionalInput.x;
            float velocityX = _characterMovement.CharacterVelocity.x;

            bool holdTowardsWall_L = leftWall && inputX < -.5f && velocityX <= 0;
            bool holdTowardsWall_R = rightWall && inputX > .5f && velocityX >= 0;

            if (CanWallCling && Airborne && (holdTowardsWall_L || holdTowardsWall_R))
                CurrentCharacterState = CharacterState.WallSlide;

            if (CurrentCharacterState is CharacterState.WallSlide && 
                _characterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
                CurrentCharacterState = CharacterState.Fall;

            if (CanWallCling && CurrentCharacterState is not CharacterState.Rise and not CharacterState.Dash &&
                _characterInput.InputState.WallClingTrigger == InputActionPhase.Performed)
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
