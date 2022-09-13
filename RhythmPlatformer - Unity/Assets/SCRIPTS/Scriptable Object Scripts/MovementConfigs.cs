using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Movement Configs", menuName = "Custom/Movement Configs")]
    public class MovementConfigs : ScriptableObject
    {
        #region ACCELERATION CURVES

        [Header("ACCELERATION CURVES")]
        
        [Tooltip("X axis -> time (moving last point horizontally changes how long it takes for running to reach " +
                 "final speed).\n\n" + "Y axis -> multiplied with RunTopSpeed (should be equal to 1 at the end)")]
        public AnimationCurve RunAcceleration;
        
        [Tooltip("X axis -> time (moving last point horizontally changes how long it takes for dashing to reach " +
                 "final speed).\n\n" + "Y axis -> multiplied with DashTopSpeed (final value works as 'dash momentum')")]
        public AnimationCurve DashAcceleration;
        
        [Tooltip("X axis -> time (moving last point horizontally changes how long it takes for rising to reach " +
                 "final speed).\n\n" + "Y axis -> multiplied with RiseTopSpeed (should be equal to 0 at the end)")]
        public AnimationCurve RiseAcceleration;
        
        [Tooltip("X axis -> time (moving last point horizontally changes how long it takes for falling to reach " +
                "final speed).\n\n" + "Y axis -> multiplied with FallTopSpeed (should be equal to 1 at the end)")]
        public AnimationCurve FallAcceleration;

        #endregion

        #region TOP SPEEDS

        [Space]
        [Header("TOP SPEEDS")]
        
        [Tooltip("RunTopSpeed is multiplied with the y value of the RunAcceleration curve over the length of the " +
                 "curve's duration (x axis). After this duration has elapsed, the curve's final value continues to" +
                 "be multiplied with RunTopSpeed.")]
        public float RunTopSpeed;
        
        [Tooltip("DashTopSpeed is multiplied with the y value of the DashAcceleration curve over the length of the " +
                 "curve's duration (x axis). After this duration has elapsed, the curve's final value continues to" +
                 "be multiplied with DashTopSpeed.")]
        public float DashTopSpeed;
        
        [Tooltip("RiseTopSpeed is multiplied with the y value of the RiseAcceleration curve over the length of the " +
                 "curve's duration (x axis). After this duration has elapsed, the curve's final value continues to" +
                 "be multiplied with RiseTopSpeed.")]
        public float RiseTopSpeed;
        
        [Tooltip("FallTopSpeed is multiplied with the y value of the FallAcceleration curve over the length of the " +
                 "curve's duration (x axis). After this duration has elapsed, the curve's final value continues to" +
                 "be multiplied with FallTopSpeed.")]
        public float FallTopSpeed;
        
        [Tooltip("After wall drag has reduced any initial wall-slide velocity enough, wall slide velocity eventually " +
                 "settles on WallSlideFallTopSpeed.\n\n" +
                 "(If the character starts the wall slide with some vertical velocity, up or down, wall drag will gradually " +
                 "decrease it and the character will begin to slide downwards in a state called WallSlideFall)")]
        public float WallSlideFallTopSpeed;
        
        [Tooltip("'Flat' amount of force added in direction of input on the x axis while rising or falling")]
        public float AirDriftSpeed;

        #endregion

        #region DRAG

        [Space]
        [Header("DRAG")]
        
        [Tooltip("Drag value while character is airborne. AirDrag can work in the same or opposite direction than " +
                 "AirDriftSpeed, depending on the player's directional input")]
        public float AirDrag;
        
        [Space]
        [Tooltip("Default drag value if player does not input an x-axis directional input during a wall slide")]
        public float DefaultGroundDrag;
        
        [Tooltip("Reduced drag value if player holds directional input in direction of a ground slide")]
        public float ReducedGroundDragFactor;
        
        [Tooltip("Increased drag value if player holds directional input against the direction of a ground slide")]
        public float IncreasedGroundDragFactor;
        
        [Space]
        [Tooltip("Default drag value if player does not input an x-axis directional input during a wall slide")]
        public float DefaultWallDrag;
        
        [Tooltip("Reduced drag value if player holds directional input in direction of a wall slide")]
        public float ReducedWallDrag;
        
        [Tooltip("Increased drag value if player holds directional input against the direction of a wall slide")]
        public float IncreasedWallDrag;
        
        #endregion

        #region MISCELLANEOUS

        [Space] 
        [Header("MISC")] 
        [Tooltip("Time window while running during which, if the player switches direction input to the opposite " +
                 "direction, the character will immediately turn but not lose their current speed (i.e. 'dash dance')")]
        public float RunTurnWindow = .1f;
        
        [Tooltip("Maximum duration the character can wall-cling at a time. " +
                 "Recharges any time the character is not wall-clinging")]
        public float WallClingMaxDuration = .5f;
        
        [Tooltip("Modifier multiplied with rise velocity if player holds down while grounded when jump comes out")]
        public float CrouchJumpSpeedModifier;
        
        [Tooltip("Modifier multiplied with fall velocity if player moves direction input down while falling " +
                 "(moving stick down before fall starts will not result in fast fall, like in Smash games)")]
        public float FastFallSpeedModifier = 2;
        
        [Tooltip("Modifier applied to character's general velocity while character is in dash windup state")]
        [Range(0,1)] public float DashWindupVelocityMod;

        #endregion
    }
}
