using UnityEngine;

namespace Structs
{
    [System.Serializable]
    public struct Waypoint
    {
        [SerializeField] private Vector2 _coords;

        public Vector2 Coords
        {
            get => _coords;
            set => _coords = value;
        }
        
        public float Pause;

        public Waypoint(Vector2 coords, float pause)
        {
            _coords = coords;
            Pause = pause;
        }
    }
}

