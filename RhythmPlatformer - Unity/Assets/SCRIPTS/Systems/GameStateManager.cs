using Interfaces;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private UpdateType _startUpdateType;
    
        public static UpdateType s_ActiveUpdateType;
        
#if UNITY_EDITOR
        public static bool s_DebugMode;
#endif

        private void Start() => s_ActiveUpdateType = _startUpdateType;
    }
}
