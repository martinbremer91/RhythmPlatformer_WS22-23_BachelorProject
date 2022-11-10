using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(menuName = "Custom/Camera Configs", fileName = "Camera Configs")]
    public class CameraConfigs : ScriptableObject
    {
        public Vector2 CharacterMovementBoundaries;
        
        public float _smoothTime;
        public float MaxSpeed;
        public float MaxSize;
        public float MinSize;
    }
}
