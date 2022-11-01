using UnityEngine;

namespace Structs
{
    [System.Serializable]
    public struct Waypoint
    {
        public Transform coords;
        public float pause;

        public Waypoint(Transform coords, float pause)
        {
            this.coords = coords;
            this.pause = pause;
        }
    }
}

