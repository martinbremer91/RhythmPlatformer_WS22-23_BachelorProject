using GameplaySystems;
using UnityEditor;
using UnityEngine;
using Structs;

namespace Editor
{
    [CustomEditor(typeof(MovementRoutine))]
    public class MovementRoutineEditor : UnityEditor.Editor
    {
        private MovementRoutine script;

        private void OnEnable()
        {
            script = (MovementRoutine)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate Waypoint"))
            {
                int waypointIndex = script.transform.childCount;

                GameObject waypointObj = new GameObject("Waypoint_" + waypointIndex);
                waypointObj.transform.SetParent(script.transform);

                Waypoint waypoint = new Waypoint(waypointObj.transform, 0);

                script.Waypoints.Add(waypoint);
            }

            base.OnInspectorGUI();
        }
    }
}

