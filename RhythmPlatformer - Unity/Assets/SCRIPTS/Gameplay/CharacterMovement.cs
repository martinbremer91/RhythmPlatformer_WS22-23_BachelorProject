using Scriptable_Object_Scripts;
using UnityEngine;

namespace Gameplay
{
    public class CharacterMovement : GameplayComponent
    {
        #region REFERENCES
        
        [SerializeField] private MovementConfigs movementConfigs;
        [SerializeField] private Rigidbody2D rb;

        #endregion

        public static Vector2 CharacterVelocity { get; private set; }

        public static float RunVelocity;
        public static float LandVelocity;
        public static float WallSlideVelocity;
        public static Vector2 DashVelocity;
        public static Vector2 RiseVelocity;
        public static Vector2 FallVelocity;

        private static float runTopSpeed;
        private static float riseTopSpeed;
        private static float fallTopSpeed;
        private static float airDriftSpeed;

        public static float RiseSpeedMod;

        private static float airDrag;
        private static float defaultSurfaceDrag;
        private static float reducedSurfaceDragFactor;
        private static float increasedSurfaceDragFactor;

        private static float crouchJumpVerticalSpeedModifier;
        private static float riseAirDriftPoint;

        // X = current time along animation curve. Y = time of last key in animation curve (i.e. length)
        public static Vector2 RunCurveTracker;
        public static Vector2 DashCurveTracker;
        public static Vector2 RiseCurveTracker;
        public static Vector2 FallCurveTracker;

        private void Awake() => GetMovementData();

        private void GetMovementData()
        {
            RunCurveTracker.y = movementConfigs.RunAcceleration.keys[^1].time;
            DashCurveTracker.y = movementConfigs.DashAcceleration.keys[^1].time;
            RiseCurveTracker.y = movementConfigs.RiseAcceleration.keys[^1].time;
            FallCurveTracker.y = movementConfigs.FallAcceleration.keys[^1].time;
            
            runTopSpeed = movementConfigs.RunTopSpeed;
            riseTopSpeed = movementConfigs.RiseTopSpeed;
            fallTopSpeed = movementConfigs.FallTopSpeed;
            airDriftSpeed = movementConfigs.AirDriftSpeed;

            airDrag = movementConfigs.AirDrag;

            defaultSurfaceDrag = movementConfigs.DefaultSurfaceDrag;
            reducedSurfaceDragFactor = movementConfigs.ReducedSurfaceDragFactor;
            increasedSurfaceDragFactor = movementConfigs.IncreasedSurfaceDragFactor;

            crouchJumpVerticalSpeedModifier = movementConfigs.CrouchJumpSpeedModifier;
            riseAirDriftPoint = movementConfigs.RiseAirDriftPoint;
        }

        public override void OnUpdate()
        {
            CharacterVelocity = GetCharacterVelocity();
            rb.velocity = CharacterVelocity;

            Vector2 GetCharacterVelocity() => new (RunVelocity + LandVelocity + FallVelocity.x + RiseVelocity.x, 
                WallSlideVelocity + FallVelocity.y + RiseVelocity.y);
        }

        public void Run()
        {
            int directionMod = CharacterStateController.FacingLeft ? -1 : 1;
            RunVelocity = directionMod * movementConfigs.RunAcceleration.Evaluate(RunCurveTracker.x) * runTopSpeed;
        }

        public void Fall()
        {
            float drift = CharacterInput.InputState.DirectionalInput.x * airDriftSpeed;

            float xVelocity = FallVelocity.x == 0 ? drift : 
                FallVelocity.x > 0 ? FallVelocity.x - (airDrag + drift) * Time.deltaTime : 
                FallVelocity.x + (airDrag + drift) * Time.deltaTime;
            
            float yVelocity = -movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.x) * fallTopSpeed; 
            
            FallVelocity = new(xVelocity, yVelocity);
        }

        public void Rise()
        {
            RiseVelocity = new(RiseVelocity.x,
                movementConfigs.RiseAcceleration.Evaluate(RiseCurveTracker.x) * riseTopSpeed * RiseSpeedMod);

            if (RiseCurveTracker.x > riseAirDriftPoint * RiseCurveTracker.y)
                RiseVelocity.x += CharacterInput.InputState.DirectionalInput.x * airDriftSpeed * Time.deltaTime;
        }

        public void Land()
        {
            int directionMod = CharacterVelocity.x > 0 ? 1 : -1;
            LandVelocity = Mathf.Abs(LandVelocity) > .05f ? 
                LandVelocity - directionMod * GetCurrentSurfaceDrag(false) * Time.deltaTime : 0;
        }
        
        public void WallSlide()
        {
            int directionMod = CharacterVelocity.y < 0 ? -1 : 1;
            WallSlideVelocity = Mathf.Abs(WallSlideVelocity) > .05f ? 
                    WallSlideVelocity - directionMod * GetCurrentSurfaceDrag(true) * Time.deltaTime : 0;
        }

        private float GetCurrentSurfaceDrag(bool wallSlide)
        {
            float slideAxisInput = wallSlide ? CharacterInput.InputState.DirectionalInput.y : 
                CharacterInput.InputState.DirectionalInput.x;
            float slideAxisVelocity = wallSlide ? CharacterVelocity.y : CharacterVelocity.x;
            float currentSurfaceDrag = defaultSurfaceDrag;

            currentSurfaceDrag *= slideAxisInput == 0 || slideAxisVelocity == 0 ? 1 :
                slideAxisVelocity * slideAxisInput > 0 ? reducedSurfaceDragFactor : increasedSurfaceDragFactor;

            return currentSurfaceDrag;
        }

        public static void InitializeRise()
        {
            bool crouching = CharacterStateController.Grounded && CharacterInput.InputState.DirectionalInput.y < -.5f;
            RiseSpeedMod = crouching ? crouchJumpVerticalSpeedModifier : 1;

            Vector2 riseVector = !CharacterStateController.Walled ? Vector2.up : CharacterStateController.FacingLeft ? 
                new Vector2(-1, 1).normalized : new Vector2(1, 1).normalized;

            RiseVelocity = new Vector2(riseVector.x, riseVector.y * RiseSpeedMod) * riseTopSpeed + 
                           new Vector2(CharacterVelocity.x, 0);

            CharacterVelocity = RiseVelocity;
        }

        public static void CancelHorizontalVelocity()
        {
            RunVelocity = 0;
            DashVelocity.x = 0;
            RiseVelocity.x = 0;
            FallVelocity.x = 0;
        }

        public static void CancelVerticalVelocity()
        {
            DashVelocity.y = 0;
            RiseVelocity.y = 0;
            FallVelocity.y = 0;
        }
    }
}
