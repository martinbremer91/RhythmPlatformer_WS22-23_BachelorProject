using Gameplay;
using UnityEngine;

namespace Systems
{
    public class CameraManager : GameplayComponent
    {
        [SerializeField] private Vector2[] _roomCorners;
        [SerializeField] private bool _framedMode;
    }
}
