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
        private readonly List<IUpdatable> _fixedUpdatables = new();

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (_orderOfExecutions.OrderedUpdatableTypes == null || _orderOfExecutions.OrderedFixedUpdatableTypes == null)
                throw new Exception("An Ordered Updatable Types list is null");

            bool fixedUpdate = false;
            
            if (_updatables.Any())
            {
                foreach (IUpdatable updatable in _updatables)
                {
                    if (!CheckTypeInList(updatable, _orderOfExecutions.OrderedUpdatableTypes))
                        throw new Exception(
                            "IUpdatable type not found in corresponding CustomOrderOfExecution list: " + updatable);
                }
                
                _updatables.Sort(SortByClass);
            }

            fixedUpdate = true;
            
            if (_fixedUpdatables.Any())
            {
                foreach (IUpdatable updatable in _fixedUpdatables)
                {
                    if (!CheckTypeInList(updatable, _orderOfExecutions.OrderedFixedUpdatableTypes))
                        throw new Exception(
                            "IUpdatable type not found in corresponding CustomOrderOfExecution list: " + updatable);
                }

                _fixedUpdatables.Sort(SortByClass);
            }
            
            bool CheckTypeInList(IUpdatable in_updatable, Type[] in_typesArray) =>
                in_typesArray.Any(t => t == in_updatable.GetType());
            
            int SortByClass(IUpdatable in_a, IUpdatable in_b)
            {
                Type[] typesArray = fixedUpdate
                    ? _orderOfExecutions.OrderedFixedUpdatableTypes
                    : _orderOfExecutions.OrderedUpdatableTypes;
                
                Type aType = typesArray.FirstOrDefault(t => t == in_a.GetType());
                Type bType = typesArray.FirstOrDefault(t => t == in_b.GetType());

                int aIndex = -1;
                int bIndex = -1;

                for (int i = 0; i < typesArray.Length; i++)
                {
                    if (aType == typesArray[i])
                        aIndex = i;
                    if (bType == typesArray[i])
                        bIndex = i;

                    if (aIndex >= 0 && bIndex >= 0)
                        break;
                }

                if (aIndex < 0 || bIndex < 0)
                    throw new Exception("Updatable type sorting failed");
            
                return aIndex > bIndex ? 1 : aIndex < bIndex ? -1 : 0;
            }
        }

        public void RegisterUpdatable(IUpdatable in_updatable, bool in_fixedUpdate)
        {
            if (!in_fixedUpdate)
                _updatables.Add(in_updatable);
            else
                _fixedUpdatables.Add(in_updatable);
        }

        public void DeregisterUpdatable(IUpdatable in_updatable, bool in_fixedUpdate)
        {
            if (!in_fixedUpdate)
                _updatables.Remove(in_updatable);
            else
                _fixedUpdatables.Remove(in_updatable);
        }

        private void Update()
        {
            foreach (IUpdatable updatable in _updatables)
            {
                if (GameStateManager.s_ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.CustomUpdate();
            }
        }
        
        private void FixedUpdate()
        {
            foreach (IUpdatable updatable in _fixedUpdatables)
            {
                if (GameStateManager.s_ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.CustomUpdate();
            }
        }
    }
}
