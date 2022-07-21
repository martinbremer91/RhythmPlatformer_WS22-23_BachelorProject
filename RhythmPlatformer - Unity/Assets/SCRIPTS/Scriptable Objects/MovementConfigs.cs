using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "Movement Configs", menuName = "PC/Movement Configs")]
    public class MovementConfigs : ScriptableObject
    {
        public AnimationCurve runAcceleration;
        public AnimationCurve dashAcceleration;
        public AnimationCurve riseAcceleration;
        public AnimationCurve fallAcceleration;
    }
}
