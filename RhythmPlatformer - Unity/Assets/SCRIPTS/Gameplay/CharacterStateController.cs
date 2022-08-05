using System.Threading.Tasks;
using Scriptable_Object_Scripts;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

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
            set => SetCharacterState(value);
        }

        private bool _jumpSquat;
        private bool _dashWindup;
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
                    if (WallClingTimer >= wallClingMaxDuration)
                    {
                        _canWallCling = false;
                        CurrentCharacterState = CharacterState.Fall;
                    }
                    break;
                case CharacterState.WallSlide:
                    if (WallClingTimer >= wallClingMaxDuration)
                    {
                        _canWallCling = false;
                        CurrentCharacterState = CharacterState.Fall;
                        break;
                    }
                    if (!NearWall_L && !NearWall_R)
                        CurrentCharacterState = CharacterState.Fall;
                    break;
            }
            
            if (NearWall_L || NearWall_R)
                NearWallChecks();

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

            void NearWallChecks()
            {
                float inputX = _characterInput.InputState.DirectionalInput.x;
                float velocityX = _characterMovement.CharacterVelocity.x;
                
                bool holdTowardsWall_L = NearWall_L && inputX < -.5f && velocityX <= 0;
                bool holdTowardsWall_R = NearWall_R && inputX > .5f && velocityX >= 0;
            
                if (Airborne && holdTowardsWall_L)
                    SetWalledState(false);
                if (Airborne && holdTowardsWall_R)
                    SetWalledState(true);
            
                if (Walled && _characterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
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
                    WallClingTimer = Mathf.Min(WallClingTimer + Time.deltaTime, wallClingMaxDuration);
                    break;
                case CharacterState.WallSlide:
                    WallClingTimer = Mathf.Min(WallClingTimer + Time.deltaTime, wallClingMaxDuration);;
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

        /// <summary>
        /// Checks y velocity and input direction. Sets state to WallCling, WallSlide, or does nothing.
        /// </summary>
        /// <param name="in_rightWall"></param>
        private void SetWalledState(bool in_rightWall)
        {
            if (!_canWallCling)
                return;
            
            if (_characterMovement.CharacterVelocity.y != 0)
                CurrentCharacterState = CharacterState.WallSlide;
            else if (in_rightWall && _characterInput.InputState.DirectionalInput.x > .5f || 
                     !in_rightWall && _characterInput.InputState.DirectionalInput.x < -.5f)
                CurrentCharacterState = CharacterState.WallCling;
        }
        
        #endregion
    }
}
