using UnityEngine;

namespace Gameplay
{
    public struct MovementSnapshot
    {
        public Vector2 mss_CharacterVelocity;
        public float mss_RunVelocity;
        public float mss_LandVelocity;
        public float mss_WallSlideVelocity;
        public Vector2 mss_DashDirection;
        public Vector2 mss_DashVelocity;
        public Vector2 mss_RiseVelocity;
        public Vector2 mss_FallVelocity;
        public float mss_RiseSpeedMod;
        public bool mss_YAxisReadyForFastFall;
        public bool mss_FastFalling;
        public float mss_RunCurveTrackerX;
        public float mss_DashCurveTrackerX;
        public float mss_RiseCurveTrackerX;
        public float mss_FallCurveTrackerX;
    }
}
