using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Movement Configs", menuName = "PC/Movement Configs")]
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
        public float SurfaceDrag;

        [Space]
        [Header("MISC")]
        public float WallClingMaxDuration = .5f;
    }
}
