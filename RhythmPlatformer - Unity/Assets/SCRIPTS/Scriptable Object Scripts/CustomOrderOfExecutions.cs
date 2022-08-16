using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Utility_Scripts;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "CustomOrderOfExecutions", menuName = "Custom/OrderOfExecutions")]
    public class CustomOrderOfExecutions : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private List<MonoScript> OrderedUpdatableScripts;
#endif

        private Type[] _orderedUpdatableTypes;
        public Type[] OrderedUpdatableTypes
        {
            get
            {
                if (_orderedUpdatableTypes == null)
                    LoadOrderedUpdatableTypes();

                return _orderedUpdatableTypes;
            }
        }

        private void LoadOrderedUpdatableTypes()
        {
            string savedData = PlayerPrefs.GetString(Constants.OrderedUpdatableTypesKey);
            string[] serializableTypeStrings = JsonArrayUtility.FromJson<string>(savedData);

            _orderedUpdatableTypes = new Type[serializableTypeStrings.Length];
            
            for (int i = 0; i < serializableTypeStrings.Length; i++)
                _orderedUpdatableTypes[i] = GetTypeByName(serializableTypeStrings[i]);

            if (_orderedUpdatableTypes == null || !_orderedUpdatableTypes.Any())
                throw new Exception("Could not load OrderedUpdatableTypes from PlayerPrefs or is empty");
            
            Type GetTypeByName(string typeName)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
                {
                    Type t = assembly.GetType(typeName);
                    
                    if (t != null)
                        return t;
                }

                throw new Exception("Could not find Type by given name: " + typeName);
            }
        } 

#if UNITY_EDITOR
        public void RefreshOrderedTypes()
        {
            if (!CheckOrderedUpdatables())
                throw new Exception("Non-IUpdatable type found in OrderedUpdatableScripts");

            string[] serializableTypeStrings = 
                OrderedUpdatableScripts.Select(s => s.GetClass().ToString()).ToArray();
            
            string jsonData =
                JsonArrayUtility.ToJson(serializableTypeStrings);
            PlayerPrefs.SetString(Constants.OrderedUpdatableTypesKey, jsonData);
            
            Debug.Log(PlayerPrefs.GetString(Constants.OrderedUpdatableTypesKey));

            bool CheckOrderedUpdatables()
            {
                foreach (MonoScript s in OrderedUpdatableScripts)
                {
                    if (s.GetClass().GetInterface(nameof(IUpdatable)) == null)
                        return false;
                }
            
                return true;
            }
        }
#endif
    }
}
