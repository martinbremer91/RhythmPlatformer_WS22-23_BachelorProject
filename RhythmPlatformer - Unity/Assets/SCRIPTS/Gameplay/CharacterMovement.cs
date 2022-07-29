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
        public static Vector2 DashVelocity;
        public static Vector2 RiseVelocity;
        public static Vector2 FallVelocity;

        private static float runTopSpeed;
        private static float riseTopSpeed;
        private static float fallTopSpeed;

        // X = current time along animation curve. Y = time of last key in animation curve (i.e. length)
        public static Vector2 RunCurveTracker;
        public static Vector2 DashCurveTracker;
        public static Vector2 RiseCurveTracker;
        public static Vector2 FallCurveTracker;

        private void Awake() => GetMovementCurveLengths();

        private void GetMovementCurveLengths()
        {
            RunCurveTracker.y = movementConfigs.RunAcceleration.keys[^1].time;
            DashCurveTracker.y = movementConfigs.DashAcceleration.keys[^1].time;
            RiseCurveTracker.y = movementConfigs.RiseAcceleration.keys[^1].time;
            FallCurveTracker.y = movementConfigs.FallAcceleration.keys[^1].time;
            
            runTopSpeed = movementConfigs.RunTopSpeed;
            riseTopSpeed = movementConfigs.RiseTopSpeed;
            fallTopSpeed = movementConfigs.FallTopSpeed;
        }

        public override void OnUpdate()
        {
            CharacterVelocity = GetCharacterVelocity();
            rb.velocity = CharacterVelocity;

            Vector2 GetCharacterVelocity() => new (RunVelocity + FallVelocity.x + RiseVelocity.x, 
                FallVelocity.y + RiseVelocity.y);
        }

        public void Run()
        {
            int directionMod = CharacterStateController.FacingLeft ? -1 : 1;
            RunVelocity = directionMod * movementConfigs.RunAcceleration.Evaluate(RunCurveTracker.x) * runTopSpeed;
        }

        public void Fall()
        {
            FallVelocity = new(FallVelocity.x, 
                -movementConfigs.FallAcceleration.Evaluate(FallCurveTracker.y) * fallTopSpeed);
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
