using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "CustomOrderOfExecutions", menuName = "Custom/OrderOfExecutions")]
    public class CustomOrderOfExecutions : ScriptableObject
    {
        public List<MonoScript> OrderedUpdatables;
    }
}
