using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using Structs;
using GlobalSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterMovement : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        #region REFERENCES
        
        private CharacterStateController _characterStateController;
        private CharacterInput _characterInput;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        
        [SerializeField] private MovementConfigs _movementConfigs;

        #endregion

        #region VARIABLES
        
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        private Vector2 _characterVelocity;
        public Vector2 CharacterVelocity => _characterVelocity;

        public float RunVelocity { get; set; }
        public float LandVelocity { get; set; }
        public float WallSlideVelocity { get; set; }

        private Vector2 _dashDirection;
        private Vector2 _dashVelocity;
        private Vector2 _riseVelocity;
        private Vector2 _fallVelocity;

        public Vector2 DashDirection {get => _dashDirection; set => _dashDirection = value;}
        public Vector2 DashVelocity {get => _dashVelocity; set => _dashVelocity = value;}
        public Vector2 RiseVelocity {get => _riseVelocity; set => _riseVelocity = value;}
        public Vector2 FallVelocity {get => _fallVelocity; set => _fallVelocity = value;}

        private float _runTopSpeed;
        private float _riseTopSpeed;
        private float _fallTopSpeed;
        private float _wallSlideFallTopSpeed;
        private float _airDriftSpeed;
        private float _dashTopSpeed;

        private float _riseSpeedMod;

        public float RunTopSpeed => _runTopSpeed;
        public float RiseSpeedMod => _riseSpeedMod;

        private float _airDrag;
        
        private float _defaultGroundDrag;
        private float _reducedGroundDrag;
        private float _increasedGroundDrag;

        private float _defaultWallDrag;
        private float _reducedWallDrag;
        private float _increasedWallDrag;

        private float _minSlideVelocity;
        private float _maxAirDriftCancelVelocity;

        private float _crouchJumpVerticalSpeedModifier;
        private float _fastFallSpeedModifier;
        [HideInInspector] public bool YAxisReadyForFastFall;
        [HideInInspector] public bool FastFalling;
        
        [HideInInspector] public Vector2 RunCurveTracker;
        [HideInInspector] public Vector2 DashCurveTracker;
        [HideInInspector] public Vector2 RiseCurveTracker;
        [HideInInspector] public Vector2 FallCurveTracker;

        #endregion

        #region INIT & UPDATE

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterInput = in_gameStateManager.CharacterInput;
            _movementConfigs = in_gameStateManager.MovementConfigs;

            GetMovementData();
        } 

        private void GetMovementData()
        {
            RunCurveTracker.y = _movementConfigs.RunAcceleration.keys[^1].time;
            DashCurveTracker.y = _movementConfigs.DashAcceleration.keys[^1].time;
            RiseCurveTracker.y = _movementConfigs.RiseAcceleration.keys[^1].time;
            FallCurveTracker.y = _movementConfigs.FallAcceleration.keys[^1].time;
            
            _runTopSpeed = _movementConfigs.RunTopSpeed;
            _riseTopSpeed = _movementConfigs.RiseTopSpeed;
            _fallTopSpeed = _movementConfigs.FallTopSpeed;
            _wallSlideFallTopSpeed = _movementConfigs.WallSlideFallTopSpeed;
            _airDriftSpeed = _movementConfigs.AirDriftSpeed;
            _dashTopSpeed = _movementConfigs.DashTopSpeed;

            _airDrag = _movementConfigs.AirDrag;

            _defaultGroundDrag = _movementConfigs.DefaultGroundDrag;
            _reducedGroundDrag = _movementConfigs.ReducedGroundDragFactor;
            _increasedGroundDrag = _movementConfigs.IncreasedGroundDragFactor;

            _defaultWallDrag = _movementConfigs.DefaultWallDrag;
            _reducedWallDrag = _movementConfigs.ReducedWallDrag;
            _increasedWallDrag = _movementConfigs.IncreasedWallDrag;
            
            _minSlideVelocity = _movementConfigs.MinSlideVelocity;
            _maxAirDriftCancelVelocity = _movementConfigs.MaxAirDriftCancelVelocity;

            _crouchJumpVerticalSpeedModifier = _movementConfigs.CrouchJumpSpeedModifier;
            _fastFallSpeedModifier = _movementConfigs.FastFallSpeedModifier;
        }

        public void CustomUpdate()
        {
#if UNITY_EDITOR
            if (GameStateManager.s_DebugMode)
            {
                _rigidbody2D.velocity = _characterInput.InputState.DirectionalInput * 10;
                return;
            }
#endif
            
            _characterVelocity = GetCharacterVelocity();

            if (_characterStateController.DashWindup)
                _characterVelocity *= _movementConfigs.DashWindupVelocityMod;
            
            _rigidbody2D.velocity = _characterVelocity;

            Vector2 GetCharacterVelocity() =>
                new(RunVelocity + LandVelocity + _fallVelocity.x + _riseVelocity.x +
                    _dashVelocity.x, WallSlideVelocity + _fallVelocity.y + _riseVelocity.y + _dashVelocity.y);
        }

        #endregion

        #region MOVE FUNCTIONS

        public void Run()
        {
            if (CharacterVelocity.x * _characterInput.InputState.DirectionalInput.x < 0)
                _characterStateController.CheckFacingOrientation();

            int directionMod = _characterStateController.FacingLeft ? -1 : 1;
            
            float velocity = RunCurveTracker.x < RunCurveTracker.y ?
                _movementConfigs.RunAcceleration.Evaluate(RunCurveTracker.x) * _runTopSpeed : _runTopSpeed;

            if (velocity > Mathf.Abs(RunVelocity) || CharacterVelocity.x * _characterInput.InputState.DirectionalInput.x < 0)
                RunVelocity = directionMod * velocity;
        }
        
        // TODO: Consolidate Fall() and Rise() overlapping functionality into a separate function
        public void Fall()
        {
            float xInput = _characterInput.InputState.DirectionalInput.x;

            if (Mathf.Abs(_fallVelocity.x) <= _maxAirDriftCancelVelocity)
            {
                if (_fallVelocity.x * xInput < 0)
                    _fallVelocity.x = 0;
            }

            float drift = xInput * _airDriftSpeed;

            float xVelocity = _fallVelocity.x == 0 ? drift : 
                _fallVelocity.x > 0 ? _fallVelocity.x - (_airDrag + drift) * Time.fixedDeltaTime : 
                _fallVelocity.x + (_airDrag + drift) * Time.fixedDeltaTime;

            if (!FastFalling)
            {
                if (!YAxisReadyForFastFall && _characterInput.InputState.DirectionalInput.y >= -.5f)
                    YAxisReadyForFastFall = true;
                if (YAxisReadyForFastFall && _characterInput.InputState.DirectionalInput.y < -.5f)
                    FastFalling = true;
            }
            
            float yVelocity = 
                -_movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.x) * 
                (FastFalling ? _fastFallSpeedModifier : 1) * _fallTopSpeed; 
            
            _fallVelocity = new(xVelocity, yVelocity);
        }

        public void Rise()
        {
            float xInput = _characterInput.InputState.DirectionalInput.x;
            float drift = xInput * _airDriftSpeed;

            float xVelocity = _riseVelocity.x == 0 ? drift : 
                _riseVelocity.x > 0 ? _riseVelocity.x - (_airDrag + drift) * Time.fixedDeltaTime : 
                _riseVelocity.x + (_airDrag + drift) * Time.fixedDeltaTime;
            
            float yVelocity = _movementConfigs.RiseAcceleration.Evaluate(RiseCurveTracker.x) * _riseTopSpeed; 
            
            _riseVelocity = new(xVelocity, yVelocity);
        }

        public void Land()
        {
            int dragDirectionMod = _characterVelocity.x > 0 ? 1 : -1;
            LandVelocity = Mathf.Abs(LandVelocity) > _minSlideVelocity ?
                LandVelocity - dragDirectionMod * GetCurrentGroundDrag() * Time.fixedDeltaTime : 0;
        }
        
        public void WallSlide()
        {
            bool wallSlideFalling = _characterInput.InputState.WallClingTrigger != InputActionPhase.Performed &&
                                    WallSlideVelocity <= 0 && WallSlideVelocity > -_wallSlideFallTopSpeed;

            float drag = wallSlideFalling ? 0 : GetCurrentWallDrag();
            float velocity;
            
            if (wallSlideFalling)
            {
                velocity = -_movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.x) * _wallSlideFallTopSpeed;
                
                if (FallCurveTracker.x < FallCurveTracker.y)
                    FallCurveTracker.x += Time.fixedDeltaTime;
            }
            else
            {
                velocity = WallSlideVelocity;
                
                if (Mathf.Abs(velocity) <= _minSlideVelocity)
                {
                    WallSlideVelocity = 0;
                    return;
                }
            }
            
            WallSlideVelocity = velocity + (_characterVelocity.y <= 0 ? 1 : -1) * drag * Time.fixedDeltaTime;
        }

        public void Dash() =>
            DashVelocity = DashDirection *
                           (_dashTopSpeed * _movementConfigs.DashAcceleration.Evaluate(DashCurveTracker.x));

        #endregion

        #region UTILITY FUNCTIONS

        public void TogglePausePhysics(bool pause) => _rigidbody2D.velocity = pause ? Vector2.zero : CharacterVelocity;

        private float GetCurrentGroundDrag()
        {
            float slideAxisInput = _characterInput.InputState.DirectionalInput.x;
            float slideAxisVelocity = _characterVelocity.x;

            float currentGroundDrag = slideAxisInput == 0 || slideAxisVelocity == 0 ? _defaultGroundDrag :
                slideAxisVelocity * slideAxisInput > 0 ? _reducedGroundDrag : _increasedGroundDrag;

            return currentGroundDrag;
        }

        private float GetCurrentWallDrag()
        {
            float slideAxisInput = _characterInput.InputState.DirectionalInput.y;

            bool increasedDrag = _characterInput.InputState.WallClingTrigger == InputActionPhase.Performed ||
                                 _characterVelocity.y * slideAxisInput < 0;

            float currentWallDrag = increasedDrag ? _increasedWallDrag :
                slideAxisInput == 0 || _characterVelocity.y == 0 ? _defaultWallDrag : _reducedWallDrag;

            if (increasedDrag)
                _characterStateController.IncrementWallClingTimer();
            
            return currentWallDrag;
        }

        public void InitializeRise()
        {
            bool crouching = _characterStateController.Grounded && _characterInput.InputState.DirectionalInput.y < -.5f;
            _riseSpeedMod = crouching ? _crouchJumpVerticalSpeedModifier : 1;

            Vector2 riseVector = !_characterStateController.Walled ? Vector2.up : _characterStateController.FacingLeft ? 
                new Vector2(-1, 1).normalized : new Vector2(1, 1).normalized;

            _riseVelocity = new Vector2(riseVector.x, riseVector.y * _riseSpeedMod) * _riseTopSpeed + 
                           new Vector2(_characterVelocity.x, 0);

            _characterVelocity = _riseVelocity;
        }
        
        /// <summary>
        /// Checks for different cases, initializes appropriate velocities and sets appropriate state.
        /// Possible states include Dash and Land.
        /// </summary>
        public void InitializeDash()
        {
            CancelHorizontalVelocity();
            CancelVerticalVelocity();
            DashVelocity = Vector2.zero;

            Vector2 inputDirection = _characterInput.InputState.DirectionalInput.normalized;

            int directionX = 
                inputDirection.x <= _characterInput.GameplayControlConfigs.InputDeadZone ?
                _characterStateController.FacingLeft ? -1 : 1 : 
                inputDirection.x < 0 ? -1 : 1;
            
            int directionY = Mathf.Abs(inputDirection.y) < .38f ? 0 : inputDirection.y < 0 ? -1 : 1;
            
            // "Wavedash" (grounded Dash)
            if (_characterStateController.Grounded && directionY < 1)
            {
                _characterVelocity = new Vector2((directionY < 0 ? .75f : 1) * _dashTopSpeed * directionX, 0);
                _characterStateController.CurrentCharacterState = CharacterState.Land;
                return;
            }

            bool wallDash =
                _characterStateController.Walled && _characterStateController.NearWallLeft && directionX < 0 ||
                _characterStateController.NearWallRight && directionX > 0;

            if (wallDash)
                directionX *= -1;

            DashDirection = new Vector2(directionX, directionY).normalized;
            DashVelocity = new Vector2(DashDirection.x, DashDirection.y) * _dashTopSpeed;
            
            _characterStateController.CurrentCharacterState = CharacterState.Dash;
            _characterVelocity = DashVelocity;
        }

        public MovementSnapshot GetMovementSnapshot()
        {
            MovementSnapshot ms = new()
            {
                mss_CharacterVelocity = _characterVelocity,
                mss_RunVelocity = RunVelocity,
                mss_LandVelocity = LandVelocity,
                mss_WallSlideVelocity = WallSlideVelocity,
                mss_DashDirection = _dashDirection,
                mss_DashVelocity = _dashVelocity,
                mss_RiseVelocity = _riseVelocity,
                mss_FallVelocity = _fallVelocity,
                mss_RiseSpeedMod = _riseSpeedMod,
                mss_YAxisReadyForFastFall = YAxisReadyForFastFall,
                mss_FastFalling = FastFalling,
                mss_RunCurveTrackerX = RunCurveTracker.x,
                mss_DashCurveTrackerX = DashCurveTracker.x,
                mss_RiseCurveTrackerX = RiseCurveTracker.x,
                mss_FallCurveTrackerX = FallCurveTracker.x
            };

            return ms;
        }

        public void ApplyMovementSnapshot(MovementSnapshot in_mss)
        {
            _characterVelocity = in_mss.mss_CharacterVelocity;
            RunVelocity = in_mss.mss_RunVelocity;
            LandVelocity = in_mss.mss_LandVelocity;
            WallSlideVelocity = in_mss.mss_WallSlideVelocity;
            _dashDirection = in_mss.mss_DashDirection;
            _dashVelocity = in_mss.mss_DashVelocity;
            _riseVelocity = in_mss.mss_RiseVelocity;
            _fallVelocity = in_mss.mss_FallVelocity;
            _riseSpeedMod = in_mss.mss_RiseSpeedMod;
            YAxisReadyForFastFall = in_mss.mss_YAxisReadyForFastFall;
            FastFalling = in_mss.mss_FastFalling;
            RunCurveTracker.x = in_mss.mss_RunCurveTrackerX;
            DashCurveTracker.x = in_mss.mss_DashCurveTrackerX;
            RiseCurveTracker.x = in_mss.mss_RiseCurveTrackerX;
            FallCurveTracker.x = in_mss.mss_FallCurveTrackerX;
        }

        #endregion

        #region CANCEL VELOCITY FUNCTIONS
        
        public void CancelHorizontalVelocity()
        {
            RunVelocity = 0;
            _dashDirection.x = 0;
            _riseVelocity.x = 0;
            _fallVelocity.x = 0;
        }

        public void CancelVerticalVelocity()
        {
            WallSlideVelocity = 0;
            _dashDirection.y = 0;
            _riseVelocity.y = 0;
            _fallVelocity.y = 0;
        }

        #endregion
    }
}
