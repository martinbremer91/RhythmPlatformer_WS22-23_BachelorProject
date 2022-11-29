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

        public int[] ArrivalBeats;
        public int[] DepartureBeats;

        public Waypoint(Vector2 coords)
        {
            _coords = coords;
            ArrivalBeats = new int[1];
            DepartureBeats = new int[1];
        }
    }
}

