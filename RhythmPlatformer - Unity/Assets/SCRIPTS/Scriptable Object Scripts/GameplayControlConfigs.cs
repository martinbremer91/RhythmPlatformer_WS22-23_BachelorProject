using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(menuName = "Custom/Control Configs", fileName = "Control Configs")]
    public class GameplayControlConfigs : ScriptableObject
    {
        public float InputDeadZone = .19f;
    }
}
