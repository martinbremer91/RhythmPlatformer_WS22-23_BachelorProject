using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Movement Configs", menuName = "Custom/Movement Configs")]
    public class MovementConfigs : ScriptableObject
    {
        [Header("ACCELERATION CURVES")]
        public AnimationCurve RunAcceleration;
        public AnimationCurve DashAcceleration;
        public AnimationCurve RiseAcceleration;
        public AnimationCurve FallAcceleration;

        [Space]
        [Header("TOP SPEEDS")]
        public float RunTopSpeed;
        public float DashTopSpeed;
        public float RiseTopSpeed;
        public float FallTopSpeed;
        public float WallSlideFallTopSpeed;
        public float AirDriftSpeed;

        [Space]
        [Header("DRAG")]
        public float AirDrag;
        [Space]
        public float DefaultGroundDrag;
        public float ReducedGroundDragFactor;
        public float IncreasedGroundDragFactor;
        [Space] 
        public float DefaultWallDrag;
        public float ReducedWallDrag;
        public float IncreasedWallDrag;

        [Space] 
        [Header("MISC")] 
        public float RunTurnWindow = .1f;
        public float WallClingMaxDuration = .5f;
        public float CrouchJumpSpeedModifier;
        [Range(0,1)] public float RiseAirDriftPoint;
    }
}
