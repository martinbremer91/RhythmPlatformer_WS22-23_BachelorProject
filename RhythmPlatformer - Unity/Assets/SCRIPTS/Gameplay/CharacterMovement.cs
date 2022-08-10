using System;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class CharacterMovement : GameplayComponent
    {
        #region REFERENCES
        
        [SerializeField] private MovementConfigs _movementConfigs;
        [SerializeField] private CharacterStateController _characterStateController;
        [SerializeField] private CharacterInput _characterInput;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        #endregion

        #region VARIABLES

        private Vector2 _characterVelocity;
        public Vector2 CharacterVelocity => _characterVelocity;

        public float RunVelocity { get; set; }
        public float LandVelocity { get; set; }
        public float WallSlideVelocity { get; set; }
        public float DashSpeed { private get; set; }

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

        private float _crouchJumpVerticalSpeedModifier;
        private float _riseAirDriftPoint;

        // X = current time along animation curve. Y = time of last key in animation curve (i.e. length)
        [HideInInspector] public Vector2 RunCurveTracker;
        [HideInInspector] public Vector2 DashCurveTracker;
        [HideInInspector] public Vector2 RiseCurveTracker;
        [HideInInspector] public Vector2 FallCurveTracker;

        #endregion

        #region INIT & UPDATE
        
        private void Awake() => GetMovementData();

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

            _crouchJumpVerticalSpeedModifier = _movementConfigs.CrouchJumpSpeedModifier;
            _riseAirDriftPoint = _movementConfigs.RiseAirDriftPoint;
        }

        public override void OnUpdate()
        {
            _characterVelocity = GetCharacterVelocity();
            _rigidbody2D.velocity = _characterVelocity;

            Vector2 GetCharacterVelocity() => 
                new (RunVelocity + LandVelocity + _fallVelocity.x + _riseVelocity.x + 
                     _dashVelocity.x, WallSlideVelocity + _fallVelocity.y + _riseVelocity.y + _dashVelocity.y);
        }

        #endregion

        #region MOVE FUNCTIONS

        public void Run()
        {
            int directionMod = _characterStateController.FacingLeft ? -1 : 1;
            float velocity = _movementConfigs.RunAcceleration.Evaluate(RunCurveTracker.x) * _runTopSpeed;

            if (velocity > Mathf.Abs(RunVelocity))
                RunVelocity = directionMod * velocity;
        }

        public void Fall()
        {
            float drift = _characterInput.InputState.DirectionalInput.x * _airDriftSpeed;

            float xVelocity = _fallVelocity.x == 0 ? drift : 
                _fallVelocity.x > 0 ? _fallVelocity.x - (_airDrag + drift) * Time.deltaTime : 
                _fallVelocity.x + (_airDrag + drift) * Time.deltaTime;
            
            float yVelocity = -_movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.x) * _fallTopSpeed; 
            
            _fallVelocity = new(xVelocity, yVelocity);
        }

        public void Rise()
        {
            _riseVelocity = new(_riseVelocity.x,
                _movementConfigs.RiseAcceleration.Evaluate(RiseCurveTracker.x) * _riseTopSpeed * _riseSpeedMod);

            if (RiseCurveTracker.x > _riseAirDriftPoint * RiseCurveTracker.y)
                _riseVelocity.x += _characterInput.InputState.DirectionalInput.x * _airDriftSpeed * Time.deltaTime;
        }

        public void Land()
        {
            int directionMod = _characterVelocity.x > 0 ? 1 : -1;
            LandVelocity = Mathf.Abs(LandVelocity) > .05f ? 
                LandVelocity - directionMod * GetCurrentGroundDrag() * Time.deltaTime : 0;
        }
        
        public void WallSlide()
        {
            bool wallSlideFalling = _characterInput.InputState.WallClingTrigger != InputActionPhase.Performed &&
                                    WallSlideVelocity <= 0 && WallSlideVelocity > -_wallSlideFallTopSpeed;

            float drag = wallSlideFalling ? 0 : GetCurrentWallDrag();
            
            float velocity = wallSlideFalling ? -_movementConfigs.FallAcceleration
                    .Evaluate(FallCurveTracker.x) * _wallSlideFallTopSpeed : 
                WallSlideVelocity; 
            
            WallSlideVelocity = velocity + (_characterVelocity.y <= 0 ? 1 : -1) * drag * Time.deltaTime;

            if (wallSlideFalling && FallCurveTracker.x < FallCurveTracker.y)
                FallCurveTracker.x += Time.deltaTime;
        }

        public void Dash() => DashVelocity = DashDirection * _dashTopSpeed *
                                             _movementConfigs.DashAcceleration.Evaluate(DashCurveTracker.x);

        #endregion

        #region UTILITY FUNCTIONS
        
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
        
        public void SetDashDirection()
        {
            Vector2 inputDirection = _characterInput.InputState.DirectionalInput.normalized;

            if (inputDirection.y <= -.95f)
            {
                // Bounce on ground?
                DashDirection = Vector2.zero;
                return;
            }

            int directionX = inputDirection == Vector2.zero || inputDirection.y >= .95f ?
                _characterStateController.FacingLeft ? -1 : 1 : inputDirection.x < 0 ? -1 : 1;
            int directionY = Mathf.Abs(inputDirection.y) < .38f ? 0 : inputDirection.y < 0 ? -1 : 1;

            if (_characterStateController.Walled && _characterStateController.NearWall_L && directionX < 0 ||
                _characterStateController.NearWall_R && directionX > 0)
            {
                // dashing into wall while walled "bounces" you off it (gives you dashCurve.Evaluate(DashCurveTracker.y) *
                // dashTopSpeed in opposite direction)
                DashDirection = Vector2.zero;
                return;
            }

            DashDirection = new Vector2(directionX, directionY).normalized;
        }

        public void FinalizeDash()
        {
            // check collisions (maybe call HandleCollisionStateChange?)
            // throw new NotImplementedException();
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
