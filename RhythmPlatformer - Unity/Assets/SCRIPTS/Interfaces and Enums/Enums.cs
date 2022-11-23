using System;

namespace Interfaces_and_Enums
{
    [Flags]
    public enum UpdateType
    {
        Nothing = 0,
        MenuTransition = 1,
        GamePlay = 2,
        Paused = 4,
        Always = ~0
    }
    
    public enum BeatState
    {
        Off,
        Active
    }
    
    public enum CollisionCheck
    {
        None = 0,
        Ground = 1,
        Ceiling = 2,
        RightWall = 3,
        LeftWall = 4
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
