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
        [SerializeField] private List<MonoScript> OrderedFixedUpdatableScripts;
#endif

        [SerializeField] private TextAsset OrderedUpdatableTypesJson;
        [SerializeField] private TextAsset OrderedFixedUpdatableTypesJson;
        
        private Type[] _orderedUpdatableTypes;
        public Type[] OrderedUpdatableTypes
        {
            get
            {
                if (_orderedUpdatableTypes == null)
                    LoadOrderedUpdatableTypes(false);

                return _orderedUpdatableTypes;
            }
        }
        
        private Type[] _orderedFixedUpdatableTypes;
        public Type[] OrderedFixedUpdatableTypes
        {
            get
            {
                if (_orderedFixedUpdatableTypes == null)
                    LoadOrderedUpdatableTypes(true);

                return _orderedFixedUpdatableTypes;
            }
        }

        private void LoadOrderedUpdatableTypes(bool in_fixedUpdate)
        {
            string savedData = 
                in_fixedUpdate ? OrderedFixedUpdatableTypesJson.text : OrderedUpdatableTypesJson.text;
            
            string[] serializableTypeStrings = JsonArrayUtility.FromJson<string>(savedData);

            Type[] orderedTypes = new Type[serializableTypeStrings.Length];

            for (int i = 0; i < serializableTypeStrings.Length; i++)
                orderedTypes[i] = GetTypeByName(serializableTypeStrings[i]);

            if (orderedTypes == null)
                throw new Exception("Could not load OrderedUpdatableTypes from json");

            if (in_fixedUpdate)
                _orderedFixedUpdatableTypes = orderedTypes;
            else
                _orderedUpdatableTypes = orderedTypes;
            
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
        public void RefreshOrderedTypes(bool in_fixedUpdate)
        {
            List<MonoScript> scriptsToRefresh = 
                in_fixedUpdate ? OrderedFixedUpdatableScripts : OrderedUpdatableScripts;

            if (!CheckOrderedUpdatables())
                throw new Exception("Non-IUpdatable type found in scripts to refresh");

            string[] serializableTypeStrings = 
                scriptsToRefresh.Select(s => s.GetClass().ToString()).ToArray();
            
            string jsonData =
                JsonArrayUtility.ToJson(serializableTypeStrings);

            string fileName = 
                in_fixedUpdate ? Constants.OrderedFixedUpdatableTypesKey : Constants.OrderedUpdatableTypesKey;
            
            System.IO.File.WriteAllText($"Assets/JsonData/{fileName}.json",
                jsonData);

            bool CheckOrderedUpdatables()
            {
                foreach (MonoScript s in scriptsToRefresh)
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
