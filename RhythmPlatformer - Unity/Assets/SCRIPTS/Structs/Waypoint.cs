using UnityEngine;

namespace Structs
{
    [System.Serializable]
    public struct Waypoint
    {
        [SerializeField] private Transform _coords;
        public Vector3 Coords => _coords.position;
        
        public float Pause;

        public Waypoint(Transform coords, float pause)
        {
            _coords = coords;
            Pause = pause;
        }
    }
}

