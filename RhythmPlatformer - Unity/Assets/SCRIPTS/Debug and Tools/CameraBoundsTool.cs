#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Systems;
using UnityEngine;
using UnityEditor;
using Utility_Scripts;

namespace Debug_and_Tools
{
    public class CameraBoundsTool : MonoBehaviour
    {
        public TextAsset CurrentRoomCamData;

        public readonly List<CamNodeObject> CamNodeObjects = new();

        [Space(10)] [SerializeField] private string _roomName;

        public class CamNodeObject
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
            if (_roomName == "" || _roomName == String.Empty)
            {
                ConfirmationDialog("Room Name must not be empty when saving", true);
                return;
            }

            if (System.IO.File.Exists($"Assets/JsonData/{_roomName}_CamData.json"))
            {
                if (!ConfirmationDialog($"Are you sure you want to overwrite {_roomName}_CamData?"))
                    return;
            }

            if (!CheckCamNodeObjectsValid())
            {
                TryCloseNodeLoop();
                if (!CheckCamNodeObjectsValid())
                {
                    ConfirmationDialog(
                        "Invalid Cam Node Object(s) found. A single closed loop is required.", true);
                    return;
                }
            }

            CamNode[] camNodes = new CamNode[CamNodeObjects.Count];

            for (int i = 0; i < camNodes.Length; i++)
            {
                CamNodeObject cno = CamNodeObjects[i];
                camNodes[i] = new CamNode(i, cno.Go.transform.position, 
                    GetIndexOfCamNodeObjectByGameObject(cno.VertN), 
                    GetIndexOfCamNodeObjectByGameObject(cno.HorN));
            }

            string jsonData = JsonArrayUtility.ToJson(camNodes);

            System.IO.File.WriteAllText($"Assets/JsonData/{_roomName}_CamData.json",
                jsonData);
            
            Debug.Log($"Saved Camera Bounds to Assets/JsonData/{_roomName}_CamData.json.\n" +
                      $"If the file doesn't show up right away, switch to another program and then back to Unity. " +
                      $"Or just give it a minute");

            bool CheckCamNodeObjectsValid()
            {
                bool closedLoop = 
                    !CamNodeObjects.Any(n => n.Go == null || n.HorN == null || n.VertN == null);

                if (!closedLoop)
                    return false;

                CamNodeObject neighbor = null;
                int i = 0;
                while (CamNodeObjects[0] != neighbor)
                {
                    if (neighbor == null)
                        neighbor = CamNodeObjects[0];
                    
                    neighbor = i % 2 == 0
                        ? GetCamNodeObjectFromGameObject(neighbor.HorN)
                        : GetCamNodeObjectFromGameObject(neighbor.VertN);

                    i++;
                }
                
                return i == CamNodeObjects.Count;
            }
        }

        public void CreateGameObjectsFromPoints()
        {
            if (!QueryDiscardAllChildren())
                return;

            string jsonData = CurrentRoomCamData.text;
            CamNode[] camNodes = JsonArrayUtility.FromJson<CamNode>(jsonData);
            GameObject[] gameObjects = new GameObject[camNodes.Length];

            for (int i = 0; i < camNodes.Length; i++)
            {
                GameObject go = new GameObject("Node_" + i);
                go.transform.SetParent(transform);
                gameObjects[i] = go;
            }

            for (int i = 0; i < camNodes.Length; i++)
            {
                CamNodeObject cno = new CamNodeObject(
                    gameObjects[i], 
                    gameObjects[camNodes[i].VerticalNeighborIndex], 
                    gameObjects[camNodes[i].HorizontalNeighborIndex]);

                gameObjects[i].transform.position = camNodes[i].Position;
                
                CamNodeObjects.Add(cno);
            }

            _roomName = CurrentRoomCamData.name.Remove(CurrentRoomCamData.name.Length - "_CamData".Length);
        }

        public void GenerateCamNodeNeighbors(GameObject in_go)
        {
            if (in_go == null || in_go == gameObject)
                GenerateStartingNodes();
            else
                GenerateMissingNeighbors();

            void GenerateStartingNodes()
            {
                GameObject origin = new GameObject("Node_0");
                GameObject verticalNeighbor = new GameObject("Node_1");
                GameObject horizontalNeighbor = new GameObject("Node_2");

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

                if (cno.VertN == null)
                {
                    GameObject verticalNeighbor = new GameObject("Node_" + CamNodeObjects.Count);
                    
                    verticalNeighbor.transform.position = in_go.transform.position + new Vector3(0, 5, 0);
                    verticalNeighbor.transform.SetParent(transform);

                    cno.VertN = verticalNeighbor;
                    CamNodeObjects.Add(
                        new CamNodeObject(verticalNeighbor, in_go, null));
                }

                if (cno.HorN == null)
                {
                    GameObject horizontalNeighbor = new GameObject("Node_" + CamNodeObjects.Count);
                    
                    horizontalNeighbor.transform.position = in_go.transform.position + new Vector3(5, 0, 0);
                    horizontalNeighbor.transform.SetParent(transform);

                    cno.HorN = horizontalNeighbor;
                    CamNodeObjects.Add(
                        new CamNodeObject(horizontalNeighbor, null, in_go));
                }
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

        public void TryCloseNodeLoop()
        {
            CamNodeObject[] missingVertNs = CamNodeObjects.Where(n => n.VertN == null).ToArray();
            CamNodeObject[] missingHorNs = CamNodeObjects.Where(n => n.HorN == null).ToArray();

            float tolerance = .1f;
            
            foreach (CamNodeObject cno in missingVertNs)
            {
                if (cno.VertN != null) 
                    continue;

                CamNodeObject validN = missingVertNs.FirstOrDefault(
                    n => n != cno && Math.Abs(
                        n.Go.transform.position.x - cno.Go.transform.position.x) < tolerance);

                if (validN == null)
                    continue;

                cno.VertN = validN.Go;
                validN.VertN = cno.Go;
            }
            
            foreach (CamNodeObject cno in missingHorNs)
            {
                if (cno.HorN != null) 
                    continue;

                CamNodeObject validN = missingHorNs.FirstOrDefault(
                    n => n != cno && Math.Abs(
                        n.Go.transform.position.y - cno.Go.transform.position.y) < tolerance);

                if (validN == null)
                    continue;

                cno.HorN = validN.Go;
                validN.HorN = cno.Go;
            }
            
            SceneView.RepaintAll();
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

        #endregion

        #region UTILITY FUNCTIONS

        private CamNodeObject GetCamNodeObjectFromGameObject(GameObject in_go) =>
            CamNodeObjects.First(n => n.Go == in_go);
        
        public bool CheckIfGameObjectIsOnCamNodeObjectsList(GameObject in_go) => 
            CamNodeObjects.Any(n => n.Go == in_go);

        public bool CheckIfGameObjectNodeHasMissingNeighbors(GameObject in_go)
        {
            CamNodeObject cno = GetCamNodeObjectFromGameObject(in_go);
            return cno.VertN == null || cno.HorN == null;
        }

        private bool ConfirmationDialog(string msg = "Confirm?", bool in_warning = false)
        {
            if (!in_warning)
            {
                bool decision = EditorUtility.DisplayDialog("Confirmation Dialog", msg, "Yes", "No");
                return decision;
            }
            
            return EditorUtility.DisplayDialog("Warning", msg, "OK");
        }

        private int GetIndexOfCamNodeObjectByGameObject(GameObject in_go) => 
            CamNodeObjects.IndexOf(GetCamNodeObjectFromGameObject(in_go));

        private void OnDrawGizmos()
        {
            if (CamNodeObjects == null || !CamNodeObjects.Any())
                return;

            Gizmos.color = Color.green;

            for (int i = 0; i < CamNodeObjects.Count; i++)
            {
                if (CamNodeObjects[i].VertN != null && 
                    GetIndexOfCamNodeObjectByGameObject(CamNodeObjects[i].VertN) > i)
                    Gizmos.DrawLine(CamNodeObjects[i].Go.transform.position, CamNodeObjects[i].VertN.transform.position);
                if (CamNodeObjects[i].HorN != null && 
                    GetIndexOfCamNodeObjectByGameObject(CamNodeObjects[i].HorN) > i)
                    Gizmos.DrawLine(CamNodeObjects[i].Go.transform.position, CamNodeObjects[i].HorN.transform.position);
            }
        }
        
        #endregion
    }
}
#endif
