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
        }

        public override void OnUpdate()
        {
            CharacterVelocity = CharacterInput.InputState.DirectionalInput * 5;
            rb.velocity = CharacterVelocity;
        }

        public static void CancelHorizontalVelocity()
        {
            RunCurveTracker.x = 0;
            DashCurveTracker.x = 0;
            RiseCurveTracker.x = 0;
            FallCurveTracker.x = 0;
        }

        public static void CancelVerticalVelocity()
        {
            DashCurveTracker.y = 0;
            RiseCurveTracker.y = 0;
            FallCurveTracker.y = 0;
        }
    }
}
