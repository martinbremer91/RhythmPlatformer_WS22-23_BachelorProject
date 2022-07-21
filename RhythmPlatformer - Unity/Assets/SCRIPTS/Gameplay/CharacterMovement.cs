using Scriptable_Objects;
using UnityEngine;

namespace Gameplay
{
    public class CharacterMovement : GameplayComponent
    {
        #region REFERENCES
        
        [SerializeField] private MovementConfigs movementConfigs;
        [SerializeField] private Rigidbody2D rb;

        #endregion

        private Vector2 velocity;
        private float runVelocity;
        
        // Animation Curve Tracking
        /// <summary>
        /// X = current time along animation curve. Y = time of last key in animation curve
        /// </summary>
        public Vector2 MovementCurveTracker;
        
        private float runCurveLength;
        private float dashCurveLength;
        private float riseCurveLength;
        private float fallCurveLength;

        private void Awake() => GetMovementCurveLengths();

        private void GetMovementCurveLengths()
        {
            runCurveLength = movementConfigs.runAcceleration.keys[^1].time;
            dashCurveLength = movementConfigs.dashAcceleration.keys[^1].time;
            riseCurveLength = movementConfigs.riseAcceleration.keys[^1].time;
            fallCurveLength = movementConfigs.fallAcceleration.keys[^1].time;
        }

        public override void OnUpdate()
        {
            
        }

        private void Idle()
        {
            
        }

        private void Run()
        {
            
        }
    }
}
