using UnityEngine;

namespace Systems
{
    [System.Serializable]
    public struct CamNode
    {
        public int Index;
        public Vector2 Position;
        
        public readonly int VerticalNeighborIndex;
        public readonly int HorizontalNeighborIndex;

        public CamNode(int in_ID, Vector2 in_pos, int in_VertID, int HorID)
        {
            Index = in_ID;
            Position = in_pos;
            VerticalNeighborIndex = in_VertID;
            HorizontalNeighborIndex = HorID;
        }
    }
}