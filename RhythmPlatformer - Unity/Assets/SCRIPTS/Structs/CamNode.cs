using UnityEngine;
using System;

namespace Structs
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

    public struct CamBoundsEdge {
        public bool Horizontal;
        
        public Vector2 NodeAPos;
        public Vector2 NodeBPos;
        
        public CamBoundsEdge(bool in_horizontal, CamNode in_nodeA, CamNode in_nodeB) {
            Horizontal = in_horizontal;
            NodeAPos = in_nodeA.Position;
            NodeBPos = in_nodeB.Position;
        }

        public CamBoundsEdge(bool in_horizontal, Vector2 in_nodeAPos, Vector2 in_nodeBPos) {
            Horizontal = in_horizontal;
            NodeAPos = in_nodeAPos;
            NodeBPos = in_nodeBPos;
        }
    }
}