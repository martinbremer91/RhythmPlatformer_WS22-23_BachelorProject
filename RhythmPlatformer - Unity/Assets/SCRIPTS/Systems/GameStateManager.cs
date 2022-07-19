using Interfaces;
using UnityEngine;

namespace Systems
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private UpdateType startUpdateType;
    
        public static UpdateType ActiveUpdateType;

        private void Start() => ActiveUpdateType = startUpdateType;
    }
}
