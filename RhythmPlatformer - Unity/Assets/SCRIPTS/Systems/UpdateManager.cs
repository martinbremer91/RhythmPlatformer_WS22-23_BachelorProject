using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Scriptable_Object_Scripts;
using UnityEngine;

namespace Systems
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance;

        [SerializeField] private CustomOrderOfExecutions _orderOfExecutions;
        private readonly List<IUpdatable> _updatables = new();

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (_orderOfExecutions.OrderedUpdatableTypes == null || !_orderOfExecutions.OrderedUpdatableTypes.Any())
                throw new Exception("Ordered Updatable Types is empty or null");

            foreach (IUpdatable updatable in _updatables)
            {
                if (!CheckTypeInList(updatable))
                    throw new Exception(
                        "IUpdatable type not found in CustomOrderOfExecution.OrderedUpdatables: " + updatable);
            }
            
            _updatables.Sort(SortByClass);

            int SortByClass(IUpdatable in_a, IUpdatable in_b)
            {
                Type aType =
                    _orderOfExecutions.OrderedUpdatableTypes.FirstOrDefault(t => t == in_a.GetType());
                Type bType =
                    _orderOfExecutions.OrderedUpdatableTypes.FirstOrDefault(t => t == in_b.GetType());

                int aIndex = -1;
                int bIndex = -1;

                for (int i = 0; i < _orderOfExecutions.OrderedUpdatableTypes.Length; i++)
                {
                    if (aType == _orderOfExecutions.OrderedUpdatableTypes[i])
                        aIndex = i;
                    if (bType == _orderOfExecutions.OrderedUpdatableTypes[i])
                        bIndex = i;

                    if (aIndex >= 0 && bIndex >= 0)
                        break;
                }

                if (aIndex < 0 || bIndex < 0)
                    throw new Exception("Updatable type sorting failed");
            
                return aIndex > bIndex ? 1 : aIndex < bIndex ? -1 : 0;
            }
            
            bool CheckTypeInList(IUpdatable in_updatable) =>
                _orderOfExecutions.OrderedUpdatableTypes.Any(t => t == in_updatable.GetType());
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
