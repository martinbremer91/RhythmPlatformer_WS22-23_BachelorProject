using System.Linq;
using UnityEditor;
using UnityEngine;
using Utility_Scripts;

namespace Systems
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Camera _cam;

        // TODO: maybe put this in a level manager in the future?
        [SerializeField] private TextAsset _camBoundsData;

        // DEBUG VALUES ;; TODO: DELETE
        private Vector2 _lowerLeftBounds;
        private Vector2 _upperLeftBounds;
        private Vector2 _upperRightBounds;
        private Vector2 _lowerRightBounds;

        [SerializeField] private Transform _characterTf;
        [SerializeField] private Vector3 _characterMovementBoundaries;

        private CamNode[] _camNodes;
        private float[] _nodeCoordsX;
        private float[] _nodeCoordsY;

        private bool _characterInBoundaries;
        private Vector3 _velocity;
        [SerializeField] private float _smoothTime;
        [SerializeField] private float _maxSpeed;

        private void Awake()
        {
            GetCamNodesFromJson();
            GetNodeCoords();
        }

        private void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);

        private void GetNodeCoords()
        {
            _nodeCoordsX = _camNodes.Select(n => n.Position.x).Distinct().ToArray();
            _nodeCoordsY = _camNodes.Select(n => n.Position.y).Distinct().ToArray();
        }

        private void Update()
        {
            Vector3 characterPosition = _characterTf.position;
            Vector3 position = transform.position;
            _characterInBoundaries =
                characterPosition.x >= position.x - _characterMovementBoundaries.x && 
                characterPosition.x <= position.x + _characterMovementBoundaries.x &&
                characterPosition.y >= position.y - _characterMovementBoundaries.y && 
                characterPosition.y <= position.y + _characterMovementBoundaries.y;

            float maxX, minX, maxY, minY;

            maxX = _nodeCoordsX.Where(x => x > position.x).Min();
            minX = _nodeCoordsX.Where(x => x < position.x).Max();
            maxY = _nodeCoordsY.Where(y => y > position.y).Min();
            minY = _nodeCoordsY.Where(y => y < position.y).Max();

            // temp DEBUG
            // _lowerLeftBounds;
            // _upperLeftBounds;
            // _upperRightBounds;
            // _lowerRightBounds;
        }

        private void LateUpdate()
        {
            if (_characterInBoundaries)
                return;
            
            // TODO: instead of following the player, calculate valid target position
            
            Vector3 characterPos = _characterTf.position;
            Vector3 position = transform.position;
            
            position = 
                Vector3.SmoothDamp(position, new Vector3(characterPos.x, characterPos.y, position.z), 
                    ref _velocity, _smoothTime, _maxSpeed);
            
            transform.position = position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.activeObject != gameObject)
                return;

            Vector2 position = transform.position;
            
            Vector2 upperLeft = 
                position + new Vector2(-_characterMovementBoundaries.x, _characterMovementBoundaries.y);
            Vector2 lowerLeft = 
                position + new Vector2(-_characterMovementBoundaries.x, -_characterMovementBoundaries.y);
            Vector2 upperRight = 
                position + new Vector2(_characterMovementBoundaries.x, _characterMovementBoundaries.y);
            Vector2 lowerRight =
                position + new Vector2(_characterMovementBoundaries.x, -_characterMovementBoundaries.y);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(upperLeft, lowerLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerLeft, lowerRight);
            
            Gizmos.color = Color.green;
            
            
        }
#endif
    }
}
