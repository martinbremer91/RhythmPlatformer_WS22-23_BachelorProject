using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Scriptable_Object_Scripts;
using TMPro;
using UnityEngine;

namespace Systems
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance;

        [SerializeField] private CustomOrderOfExecutions _orderOfExecutions;
        private readonly List<IUpdatable> _updatables = new();

        // temp
        public TMP_Text text;
        private bool textDone;

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
            
            _updatables.Sort(SortByClass);
            
            int SortByClass(IUpdatable in_a, IUpdatable in_b)
            {
                if (!CheckTypeInList(in_a) || !CheckTypeInList(in_b))
                    throw new Exception("IUpdatable type not found in CustomOrderOfExecution.OrderedUpdatables");
            
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
            //temp
            string order = "";
            
            foreach (IUpdatable updatable in _updatables)
            {
                if (GameStateManager.s_ActiveUpdateType.HasFlag(updatable.UpdateType))
                    updatable.OnUpdate();

                // temp
                order += updatable.GetType().ToString() + ", ";
            }

            // temp
            if (!textDone)
            {
                textDone = true;
                text.text = order;
            }
        }
    }
}
