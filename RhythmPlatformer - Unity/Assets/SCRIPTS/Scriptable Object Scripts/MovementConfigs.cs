using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Movement Configs", menuName = "PC/Movement Configs")]
    public class MovementConfigs : ScriptableObject
    {
        public AnimationCurve RunAcceleration;
        public AnimationCurve DashAcceleration;
        public AnimationCurve RiseAcceleration;
        public AnimationCurve FallAcceleration;

        public float RunTopSpeed;
        public float DashTopSpeed;
        public float RiseTopSpeed;
        public float FallTopSpeed;

        public float WallClingMaxDuration = .5f;
    }
}
