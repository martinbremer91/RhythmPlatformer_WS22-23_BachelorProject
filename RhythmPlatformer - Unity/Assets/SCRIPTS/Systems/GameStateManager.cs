using Interfaces;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private UpdateType _startUpdateType;
    
        public static UpdateType s_ActiveUpdateType;

        private void Start() => s_ActiveUpdateType = _startUpdateType;
    }
}
