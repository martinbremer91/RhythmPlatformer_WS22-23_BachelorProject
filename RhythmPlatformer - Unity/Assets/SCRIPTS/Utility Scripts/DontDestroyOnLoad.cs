using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility_Scripts
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private static List<DontDestroyOnLoad> s_instances;
        private static List<DontDestroyOnLoad> s_Instances => s_instances ??= new();

        public string ID;
        
        private void OnEnable()
        {
            if (ID == "")
                throw new Exception("Objects using DontDestroyOnLoad.cs must have a id string");

            if (s_Instances.Any(i => i.ID == ID))
            {
                Destroy(gameObject);
                return;
            }
            
            s_Instances.Add(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            s_Instances.Remove(this);
        }
    }
}
