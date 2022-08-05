using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Systems
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance;
        
        private readonly List<IUpdatable> _updatables = new();

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RegisterUpdatable(IUpdatable in_updatable) => _updatables.Add(in_updatable);
        public void DeregisterUpdatable(IUpdatable in_updatable) => _updatables.Remove(in_updatable);

        private void Update()
        {
            foreach (IUpdatable updatable in _updatables)
            {
                if (GameStateManager.s_ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.OnUpdate();
            }
        }
    }
}
