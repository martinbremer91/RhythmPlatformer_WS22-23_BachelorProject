using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance;
        
        private List<IUpdatable> updatables = new();

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RegisterUpdatable(IUpdatable updatable) => updatables.Add(updatable);
        public void DeregisterUpdatable(IUpdatable updatable) => updatables.Remove(updatable);

        private void Update()
        {
            foreach (IUpdatable updatable in updatables)
            {
                if (GameStateManager.ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.OnUpdate();
            }
        }
    }
}
