using Debug_and_Tools;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CameraBoundsTool))]
    public class CameraBoundsToolEditor : UnityEditor.Editor
    {
        private CameraBoundsTool obj;

        private void OnEnable() => obj = (CameraBoundsTool) target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create GameObjects from points"))
                obj.CreateGameObjectsFromPoints();
            if (GUILayout.Button("Save GameObject positions as points"))
                obj.SaveGameObjectPositionsAsPoints();
        }
    }
}
