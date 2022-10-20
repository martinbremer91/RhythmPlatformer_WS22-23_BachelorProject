using System;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private UpdateType _startUpdateType;

        private static UpdateType s_activeUpdateType;
        public static UpdateType s_ActiveUpdateType
        {
            get => s_activeUpdateType;
            set
            {
                if (s_activeUpdateType == value)
                    return;
                
                UpdateType oldValue = s_activeUpdateType;
                
                if (oldValue == UpdateType.Paused)
                    TogglePause?.Invoke(false);
                if (value == UpdateType.Paused)
                    TogglePause?.Invoke(true);

                s_activeUpdateType = value;
            }
        }

        public static Action<bool> TogglePause;

#if UNITY_EDITOR
        public static bool s_DebugMode;
#endif

        private void Start() => s_ActiveUpdateType = _startUpdateType;
    }
}
