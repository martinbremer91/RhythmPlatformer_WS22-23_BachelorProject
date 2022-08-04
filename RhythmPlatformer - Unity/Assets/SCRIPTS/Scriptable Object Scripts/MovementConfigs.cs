using UnityEngine;

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
        public float AirDriftSpeed;

        [Space]
        [Header("DRAG")]
        public float AirDrag;
        public float DefaultSurfaceDrag;
        public float ReducedSurfaceDragFactor;
        public float IncreasedSurfaceDragFactor;

        [Space]
        [Header("MISC")]
        public float WallClingMaxDuration = .5f;
        public float CrouchJumpSpeedModifier;
        [Range(0,1)] public float RiseAirDriftPoint;
        public float WallSlideFallSpeed;
    }
}
