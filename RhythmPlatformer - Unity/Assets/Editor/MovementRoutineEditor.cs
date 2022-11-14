using System.Linq;
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

        private void OnSceneGUI()
        {
            if (script.Waypoints == null || !script.Waypoints.Any())
                return;

            Handles.color = Color.green;
            
            for (int i = script.Waypoints.Count - 1; i >= 0; i--)
            {
                Waypoint waypoint = script.Waypoints[i];
                EditorGUI.BeginChangeCheck();
                Vector3 newTargetPosition = Handles.FreeMoveHandle(waypoint.Coords, Quaternion.identity, 
                    .25f, Vector3.one * .5f, Handles.CircleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(script, "Change Waypoint Position");
                    Waypoint newWaypoint = new Waypoint(newTargetPosition, waypoint.Pause);
                    script.Waypoints[i] = newWaypoint;
                }
            }
        }
    }
}

