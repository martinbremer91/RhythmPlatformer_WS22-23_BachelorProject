using System;
using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public class CharacterStateDetector : GameplayComponent
    {
        public static CharacterState CurrentCharacterState;
        
        public override void OnUpdate()
        {
            
        }
    }

    [Flags]
    public enum CharacterState
    {
        Idle = 1,
        Run = 2,
        Crouch = 4,
        Land = 8,
        Slide = 16,
        Rise = 32,
        Fall = 64,
        FastFall = 128,
        DashHorizontal = 256,
        DashDiagonal = 512,
        WallCling = 1024,
        WallSlide = 2048,
        JumpSquat = 4096,
        DashWindup = 8192,
        Grounded = Idle | Run | Crouch | Land | Slide,
        Airborne = Rise | Fall | FastFall,
        Walled = WallCling | WallSlide,
        Dashing = DashHorizontal | DashDiagonal,
        CanTurn = Idle | Run | Crouch
    }
}
