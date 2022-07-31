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
        public static float AirDriftVelocity;
        public static float WallSlideVelocity;
        public static Vector2 DashVelocity;
        public static Vector2 RiseVelocity;
        public static Vector2 FallVelocity;

        private static float runTopSpeed;
        private static float riseTopSpeed;
        private static float fallTopSpeed;
        private static float airDriftSpeed;

        private static float defaultAirDrag;
        private static float reducedAirDragFactor;
        private static float increasedAirDragFactor;
        private static float defaultSurfaceDrag;
        private static float reducedSurfaceDragFactor;
        private static float increasedSurfaceDragFactor;

        private static float crouchJumpVerticalSpeedModifier;

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

            defaultAirDrag = movementConfigs.DefaultAirDrag;
            reducedAirDragFactor = movementConfigs.ReducedAirDragFactor;
            increasedAirDragFactor = movementConfigs.IncreasedAirDragFactor;
            
            defaultSurfaceDrag = movementConfigs.DefaultSurfaceDrag;
            reducedSurfaceDragFactor = movementConfigs.ReducedSurfaceDragFactor;
            increasedSurfaceDragFactor = movementConfigs.IncreasedSurfaceDragFactor;

            crouchJumpVerticalSpeedModifier = movementConfigs.CrouchJumpSpeedModifier;
        }

        public override void OnUpdate()
        {
            CharacterVelocity = GetCharacterVelocity();
            rb.velocity = CharacterVelocity;

            Vector2 GetCharacterVelocity() => new (RunVelocity + LandVelocity + FallVelocity.x + RiseVelocity.x + 
                AirDriftVelocity, WallSlideVelocity + FallVelocity.y + RiseVelocity.y);
        }

        public void Run()
        {
            int directionMod = CharacterStateController.FacingLeft ? -1 : 1;
            RunVelocity = directionMod * movementConfigs.RunAcceleration.Evaluate(RunCurveTracker.x) * runTopSpeed;
        }

        public void Fall()
        {
            FallVelocity = new(Mathf.Abs(FallVelocity.x) > .05f ? 
                        FallVelocity.x - FallVelocity.x * GetCurrentAirDrag() * Time.deltaTime : 0,
                    -movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.x) * fallTopSpeed);
            
            ApplyAirDrift(FallVelocity.x);
        }

        public void Rise()
        {
            RiseVelocity = new(Mathf.Abs(RiseVelocity.x) > .05f ? 
                        RiseVelocity.x - RiseVelocity.x * GetCurrentAirDrag() * Time.deltaTime : 0,
                    movementConfigs.RiseAcceleration.Evaluate(RiseCurveTracker.x) * riseTopSpeed);
            
            ApplyAirDrift(RiseVelocity.x);
        }

        private float GetCurrentAirDrag()
        {
            float inputX = CharacterInput.InputState.DirectionalInput.x;
            float airborneX = FallVelocity.x + RiseVelocity.x;
            float currentAirDrag = defaultAirDrag;

            currentAirDrag *= inputX == 0 || airborneX == 0 ? 1 :
                airborneX * inputX > 0 ? reducedAirDragFactor : increasedAirDragFactor;

            return currentAirDrag;
        }

        private void ApplyAirDrift(float airborneHorizontalVelocity)
        {
            if (airborneHorizontalVelocity == 0)
                AirDriftVelocity = CharacterInput.InputState.DirectionalInput.x * airDriftSpeed;
        }

        public void Land()
        {
            int directionMod = CharacterVelocity.x > 0 ? 1 : -1;
            LandVelocity = 
                Mathf.Abs(LandVelocity) > .05f ? LandVelocity - directionMod * defaultSurfaceDrag * Time.deltaTime : 0;
        }
        
        public void WallSlide()
        {
            int directionMod = CharacterVelocity.y < 0 ? -1 : 1;
            WallSlideVelocity = Mathf.Abs(WallSlideVelocity) > .05f ? 
                    WallSlideVelocity - directionMod * defaultSurfaceDrag * Time.deltaTime : 0;
        }

        public static float GetInitialJumpVerticalSpeed()
        {
            return CharacterStateController.Grounded && CharacterInput.InputState.DirectionalInput.y < -.5f ? 
                riseTopSpeed * crouchJumpVerticalSpeedModifier: riseTopSpeed;
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
