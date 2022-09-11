using UnityEngine;

namespace Systems
{
    public struct CameraBounds
    {
        public Vector2 CurrentNW;
        public Vector2 CurrentNE;
        public Vector2 CurrentSW;
        public Vector2 CurrentSE;

        public float MaxY;
        public float MinY;
        public float MinX;
        public float MaxX;
    }
}
