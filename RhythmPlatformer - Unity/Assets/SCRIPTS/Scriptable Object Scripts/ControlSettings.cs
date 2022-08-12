using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(menuName = "Custom/Control Settings", fileName = "Control Settings")]
    public class ControlSettings : ScriptableObject
    {
        public float InputDeadZone = .19f;
    }
}
