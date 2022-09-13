using System.Linq;
using Debug_and_Tools;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CameraBoundsTool))]
    public class CameraBoundsToolEditor : UnityEditor.Editor
    {
        private CameraBoundsTool _obj;
        private GameObject _selection;

        private void OnEnable()
        {
            _obj = (CameraBoundsTool) target;
            Selection.selectionChanged += HandleSelectionChange;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("INSPECTOR MUST BE LOCKED TO USE THIS TOOL", MessageType.Warning);
            base.OnInspectorGUI();

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_obj.CurrentRoomCamData == null);
            if (GUILayout.Button("Create GameObjects from points"))
                _obj.CreateGameObjectsFromPoints();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_obj.CamNodeObjects == null || !_obj.CamNodeObjects.Any());
            if (GUILayout.Button("Save GameObject positions as points"))
                _obj.SaveGameObjectPositionsAsPoints();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
            bool blankEditor = _obj.CamNodeObjects != null && !_obj.CamNodeObjects.Any();

            EditorGUI.BeginDisabledGroup(!blankEditor && 
                                         (_selection == null ||
                                         !_obj.CheckIfGameObjectIsOnCamNodeObjectsList(_selection) || 
                                         !_obj.CheckIfGameObjectNodeHasMissingNeighbors(_selection)));
            if (GUILayout.Button(blankEditor ? "Generate Points" : "Generate Neighbors")) 
                _obj.GenerateCamNodeNeighbors(_selection);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_obj.CamNodeObjects == null || 
                                         !_obj.CamNodeObjects.Any(n => n.VertN == null || n.HorN == null));
            if (GUILayout.Button("Try to Close Node Loop"))
                _obj.TryCloseNodeLoop();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(_selection == null || _selection == _obj.gameObject ||
                                         !_obj.CheckIfGameObjectIsOnCamNodeObjectsList(_selection));
            if (GUILayout.Button("Remove Node"))
                    _obj.RemoveNode(_selection);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_obj.CamNodeObjects == null || !_obj.CamNodeObjects.Any() && _obj.transform.childCount == 0);
            if (GUILayout.Button("Force Clear List"))
                _obj.QueryDiscardAllChildren();
            EditorGUI.EndDisabledGroup();
        }

        private void HandleSelectionChange() => _selection = Selection.activeGameObject;

        private void OnDisable() => Selection.selectionChanged -= HandleSelectionChange;
    }
}
