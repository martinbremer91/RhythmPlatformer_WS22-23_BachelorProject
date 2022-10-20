using Scriptable_Object_Scripts;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CustomOrderOfExecutions))]
    public class CustomOrderOfExecutionsEditor : UnityEditor.Editor
    {
        private CustomOrderOfExecutions obj;
        
        private void OnEnable()
        {
            obj = (CustomOrderOfExecutions)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Refresh Ordered Updatable Types"))
                obj.RefreshOrderedTypes(false);
            if (GUILayout.Button("Refresh Ordered Fixed Updatable Types"))
                obj.RefreshOrderedTypes(true);
        }
    }
}
