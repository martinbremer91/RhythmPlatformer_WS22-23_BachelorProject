using UnityEngine;

[CreateAssetMenu(fileName = "CompanionConfigs", menuName = "Custom/Companion Configs")]
public class CompanionConfigs : ScriptableObject 
{ 
    public AnimationCurve FollowAccelerationCurve;
    public AnimationCurve FollowArcVelocityCurve;

    [Space(10)]
    public Vector2 CharacterOffset;
    
    [Space(10)]
    public float ReactionDelay;
    public float StartFollowDist;
    public float StopFollowDist;
    [Space(10)]
    public float FollowSpeedMax;
    public float FollowSpeedMin;
    public float MaxSpeedDist;
    public float MinSpeedDist;
    [Space(10)]
    public float FollowArcOffsetMax;
}
