using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(menuName = "Custom/Settings Configs", fileName = "SettingsConfig")]
    public class SettingsConfigs : ScriptableObject
    {
        public float InputDeadZone = .19f;
    }
}
