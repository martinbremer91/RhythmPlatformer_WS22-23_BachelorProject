using System;

namespace Interfaces_and_Enums
{
    [Flags]
    public enum UpdateType
    {
        GamePlay = 1,
        Paused = 2,
        Always = ~0
    }
    
    public enum BeatState
    {
        Off,
        Active,
        Standby
    }
    
    public enum CollisionCheck
    {
        Ground,
        Ceiling,
        RightWall,
        LeftWall
    }
    
    public enum CharacterState
    {
        Idle = 0,
        Run = 1,
        Land = 2,
        Crouch = 3,
        Rise = 4,
        Fall = 5,
        WallCling = 6,
        WallSlide = 7,
        Dash = 8
    }

    public enum SceneType
    {
        MainMenu,
        Level
    }
}
