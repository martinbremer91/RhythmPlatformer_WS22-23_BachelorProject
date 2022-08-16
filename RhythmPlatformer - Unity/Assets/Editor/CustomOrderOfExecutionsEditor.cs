using Scriptable_Object_Scripts;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CustomOrderOfExecutions))]
    public class CustomOrderOfExecutionsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomOrderOfExecutions obj = (CustomOrderOfExecutions)target;
            
            if (GUILayout.Button("Refresh Ordered Updatable Types"))
                obj.RefreshOrderedTypes();
        }
    }
}
