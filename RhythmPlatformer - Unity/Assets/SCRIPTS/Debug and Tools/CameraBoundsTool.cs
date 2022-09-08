#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Systems;
using UnityEngine;
using UnityEditor;

namespace Debug_and_Tools
{
    public class CameraBoundsTool : MonoBehaviour
    {
        [SerializeField] private TextAsset _currentLevelCameraBoundsJson;

        public readonly List<CamNodeObject> CamNodeObjects = new();

        [Space(10)] [SerializeField] private string _roomName;

        public struct CamNodeObject
        {
            public GameObject Go;
            public GameObject VertN;
            public GameObject HorN;

            public CamNodeObject(GameObject in_go, GameObject in_vertN, GameObject in_horN)
            {
                Go = in_go;
                VertN = in_vertN;
                HorN = in_horN;
            }
        }

        #region INSPECTOR BUTTONS

        public void SaveGameObjectPositionsAsPoints()
        {
            
            
            // check if _currentLevelCameraBoundsJson exists. If so, overwrite. If not, create and assign field
            // (name file after _roomName)
        }

        public void CreateGameObjectsFromPoints()
        {
            if (_currentLevelCameraBoundsJson == null)
            {
                Debug.LogWarning("Pass in JSON to generate Camera Bounds GameObjects");
                return;
            }

            if (!QueryDiscardAllChildren())
                return;
            
            Debug.Log("Creating objs");
            
            // Update _roomName 
        }

        public void GenerateCamNodeNeighbors(GameObject in_go)
        {
            if (in_go == null || in_go == gameObject)
                GenerateStartingNodes();
            else
                GenerateMissingNeighbors();

            void GenerateStartingNodes()
            {
                GameObject origin = new GameObject();
                GameObject verticalNeighbor = new GameObject();
                GameObject horizontalNeighbor = new GameObject();

                origin.name = "Node_0";
                verticalNeighbor.name = "Node_1";
                horizontalNeighbor.name = "Node_2";
                
                origin.transform.position = Vector3.zero;
                verticalNeighbor.transform.position = new Vector3(0, 5, 0);
                horizontalNeighbor.transform.position = new Vector3(5, 0, 0);
                
                origin.transform.SetParent(transform);
                verticalNeighbor.transform.SetParent(transform);
                horizontalNeighbor.transform.SetParent(transform);
                
                CamNodeObjects.Add(
                    new CamNodeObject(origin, verticalNeighbor, horizontalNeighbor));
                
                CamNodeObjects.Add(
                    new CamNodeObject(verticalNeighbor, origin, null));
                
                CamNodeObjects.Add(
                    new CamNodeObject(horizontalNeighbor, null, origin));
            }

            void GenerateMissingNeighbors()
            {
                CamNodeObject cno = GetCamNodeObjectFromGameObject(in_go);
            }
        }

        public void RemoveNode(GameObject in_go)
        {
            CamNodeObject cno = GetCamNodeObjectFromGameObject(in_go);

            if (cno.HorN != null)
            {
                CamNodeObject horN = GetCamNodeObjectFromGameObject(cno.HorN);
                horN.HorN = null;
            }
            if (cno.VertN != null)
            {
                CamNodeObject vertN = GetCamNodeObjectFromGameObject(cno.VertN);
                vertN.VertN = null;
            }

            CamNodeObjects.Remove(cno);
            DestroyImmediate(cno.Go);
        }

        #endregion

        private CamNodeObject GetCamNodeObjectFromGameObject(GameObject in_go) =>
            CamNodeObjects.First(n => n.Go == in_go);
        
        public bool CheckIfGameObjectIsOnCamNodeObjectsList(GameObject in_go) => 
            CamNodeObjects.Any(n => n.Go == in_go);

        public bool CheckIfGameObjectNodeHasMissingNeighbors(GameObject in_go)
        {
            CamNodeObject cno = GetCamNodeObjectFromGameObject(in_go);
            return cno.VertN == null || cno.HorN == null;
        }

        public bool QueryDiscardAllChildren()
        {
            if (transform.childCount > 0)
            {
                if (!ConfirmationDialog("Discard current CameraBoundTool children?"))
                    return false;

                for (int i = transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(transform.GetChild(i).gameObject);
                
                CamNodeObjects.Clear();
            }

            return true;
        }

        private bool ConfirmationDialog(string msg = "Confirm?")
        {
            bool decision = EditorUtility.DisplayDialog("Confirmation Dialog", msg, "Yes", "No");
            return decision;
        }
    }
}
#endif
