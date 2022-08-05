using Scriptable_Object_Scripts;
using UnityEngine;

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

        private Vector2 _dashVelocity;
        private Vector2 _riseVelocity;
        private Vector2 _fallVelocity;

        public Vector2 DashVelocity {get => _dashVelocity; set => _dashVelocity = value;}
        public Vector2 RiseVelocity {get => _riseVelocity; set => _riseVelocity = value;}
        public Vector2 FallVelocity {get => _fallVelocity; set => _fallVelocity = value;}

        private float _runTopSpeed;
        private float _riseTopSpeed;
        private float _fallTopSpeed;
        private float _airDriftSpeed;

        private float _riseSpeedMod;

        public float RunTopSpeed => _runTopSpeed;
        public float RiseSpeedMod => _riseSpeedMod;

        private float _airDrag;
        
        private float _defaultGroundDrag;
        private float _reducedGroundDragFactor;
        private float _increasedGroundDragFactor;

        private float _defaultWallDrag;
        private float _reducedWallDragFactor;
        private float _increasedWallDragFactor;

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
            _airDriftSpeed = _movementConfigs.AirDriftSpeed;

            _airDrag = _movementConfigs.AirDrag;

            _defaultGroundDrag = _movementConfigs.DefaultGroundDrag;
            _reducedGroundDragFactor = _movementConfigs.ReducedGroundDragFactor;
            _increasedGroundDragFactor = _movementConfigs.IncreasedGroundDragFactor;

            _defaultWallDrag = _movementConfigs.DefaultWallDrag;
            _reducedWallDragFactor = _movementConfigs.ReducedWallDrag;
            _increasedWallDragFactor = _movementConfigs.IncreasedWallDrag;

            _crouchJumpVerticalSpeedModifier = _movementConfigs.CrouchJumpSpeedModifier;
            _riseAirDriftPoint = _movementConfigs.RiseAirDriftPoint;
        }

        public override void OnUpdate()
        {
            _characterVelocity = GetCharacterVelocity();
            _rigidbody2D.velocity = _characterVelocity;

            Vector2 GetCharacterVelocity() => new (RunVelocity + LandVelocity + _fallVelocity.x + _riseVelocity.x, 
                WallSlideVelocity + _fallVelocity.y + _riseVelocity.y);
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
            float velocity = WallSlideVelocity -
                             (_characterVelocity.y <= 0 ? -1 : 1) * GetCurrentWallDrag() * Time.deltaTime;
            
            WallSlideVelocity = Mathf.Max(velocity);
        }

        #endregion

        #region UTILITY FUNCTIONS
        
        private float GetCurrentGroundDrag()
        {
            float slideAxisInput = _characterInput.InputState.DirectionalInput.x;
            float slideAxisVelocity = _characterVelocity.x;

            float currentSurfaceDrag = slideAxisInput == 0 || slideAxisVelocity == 0 ? _defaultGroundDrag :
                slideAxisVelocity * slideAxisInput > 0 ? _reducedGroundDragFactor : _increasedGroundDragFactor;

            return currentSurfaceDrag;
        }

        private float GetCurrentWallDrag()
        {
            float slideAxisInput = _characterInput.InputState.DirectionalInput.y;
            float slideAxisVelocity = _characterVelocity.y;

            float currentSurfaceDrag = slideAxisInput == 0 || slideAxisVelocity == 0 ? _defaultWallDrag :
                slideAxisVelocity * slideAxisInput > 0 ? _reducedWallDragFactor : _increasedWallDragFactor;

            return currentSurfaceDrag;
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

        #endregion

        #region CANCEL VELOCITY FUNCTIONS
        
        public void CancelHorizontalVelocity()
        {
            RunVelocity = 0;
            _dashVelocity.x = 0;
            _riseVelocity.x = 0;
            _fallVelocity.x = 0;
        }

        public void CancelVerticalVelocity()
        {
            WallSlideVelocity = 0;
            _dashVelocity.y = 0;
            _riseVelocity.y = 0;
            _fallVelocity.y = 0;
        }

        #endregion
    }
}
