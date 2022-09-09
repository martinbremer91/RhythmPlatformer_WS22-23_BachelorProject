using UnityEngine;
using System;

namespace Systems
{
    [Serializable]
    public struct CamNode
    {
        public int Index;
        public Vector2 Position;
        
        public int VerticalNeighborIndex;
        public int HorizontalNeighborIndex;

        public CamNode(int in_ID, Vector2 in_pos, int in_VertID, int in_HorID)
        {
            Index = in_ID;
            Position = in_pos;
            VerticalNeighborIndex = in_VertID;
            HorizontalNeighborIndex = in_HorID;
        }
    }
}