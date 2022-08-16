using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Scriptable_Object_Scripts;
using UnityEditor;
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
            if (!CheckOrderedUpdatables())
                throw new Exception("Non-IUpdatable type found in CustomOrderOfExecution.OrderedUpdatables");
            
            _updatables.Sort(SortByClass);

            int SortByClass(IUpdatable in_a, IUpdatable in_b)
            {
                if (!CheckTypeInList(in_a) || !CheckTypeInList(in_b))
                    throw new Exception("IUpdatable type not found in CustomOrderOfExecution.OrderedUpdatables");

                MonoScript aType =
                    _orderOfExecutions.OrderedUpdatables.FirstOrDefault(t => t.GetClass() == in_a.GetType());
                MonoScript bType =
                    _orderOfExecutions.OrderedUpdatables.FirstOrDefault(t => t.GetClass() == in_b.GetType());
                
                int aIndex = _orderOfExecutions.OrderedUpdatables.IndexOf(aType);
                int bIndex = _orderOfExecutions.OrderedUpdatables.IndexOf(bType);

                return aIndex > bIndex ? 1 : aIndex < bIndex ? -1 : 0;
            }

            bool CheckTypeInList(IUpdatable in_a) =>
                _orderOfExecutions.OrderedUpdatables.Any(t => t.GetClass() == in_a.GetType());

            bool CheckOrderedUpdatables()
            {
                foreach (MonoScript s in _orderOfExecutions.OrderedUpdatables)
                {
                    if (s.GetClass().GetInterface(nameof(IUpdatable)) == null)
                        return false;
                }

                return true;
            }
        }

        public void RegisterUpdatable(IUpdatable in_updatable) => _updatables.Add(in_updatable);
        public void DeregisterUpdatable(IUpdatable in_updatable) => _updatables.Remove(in_updatable);

        private void Update()
        {
            string order = "";
            
            foreach (IUpdatable updatable in _updatables)
            {
                if (GameStateManager.s_ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.OnUpdate();

                order += updatable.ToString();
            }
            
            Debug.Log(order);
        }
    }
}
