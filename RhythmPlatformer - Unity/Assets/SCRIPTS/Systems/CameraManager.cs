using System.Collections.Generic;
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

        [SerializeField] private Transform _characterTf;
        [SerializeField] private Vector3 _characterMovementBoundaries;

        private CamNode[] _camNodes;

        private List<float> _nodeDistances = new();
        private Vector2 _currentNW;
        private Vector2 _currentNE;
        private Vector2 _currentSW;
        private Vector2 _currentSE;

        private bool _characterInBoundaries;
        private Vector3 _velocity;
        private Vector3 _lastFramePos;
        [SerializeField] private float _smoothTime;
        [SerializeField] private float _maxSpeed;

        private void Awake()
        {
            GetCamNodesFromJson();

            for (int i = 0; i < _camNodes.Length; i++)
            {
                CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                _nodeDistances.Add(Vector3.Distance(cn.Position, transform.position));
            }
        }

        private void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);

        private void Update()
        {
            Vector3 characterPosition = _characterTf.position;
            Vector3 position = transform.position;
            _characterInBoundaries =
                characterPosition.x >= position.x - _characterMovementBoundaries.x && 
                characterPosition.x <= position.x + _characterMovementBoundaries.x &&
                characterPosition.y >= position.y - _characterMovementBoundaries.y && 
                characterPosition.y <= position.y + _characterMovementBoundaries.y;

            float tolerance = .1f;
            if (Vector3.Distance(_lastFramePos, position) > tolerance)
            {
                for (int i = 0; i < _camNodes.Length; i++)
                {
                    CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                    _nodeDistances[i] = Vector3.Distance(cn.Position, transform.position);
                }
            }

            _currentNW = 
                GetClosestNode(_camNodes.Where(n => n.Position.x <= position.x && n.Position.y >= position.y));
            _currentNE = 
                GetClosestNode(_camNodes.Where(n => n.Position.x >= position.x && n.Position.y >= position.y));
            _currentSW = 
                GetClosestNode(_camNodes.Where(n => n.Position.x <= position.x && n.Position.y <= position.y));
            _currentSE = 
                GetClosestNode(_camNodes.Where(n => n.Position.x >= position.x && n.Position.y <= position.y));

            Vector2 GetClosestNode(IEnumerable<CamNode> nodes)
            {
                return nodes.Aggregate((minD, n) => 
                        _nodeDistances[n.Index] < _nodeDistances[minD.Index] ? n : minD).Position;
            }
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
            _lastFramePos = position;
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
            
            Gizmos.color = Color.blue;
            
            Gizmos.DrawWireSphere(_currentNW, .5f);
            Gizmos.DrawWireSphere(_currentNE, .5f);
            Gizmos.DrawWireSphere(_currentSW, .5f);
            Gizmos.DrawWireSphere(_currentSE, .5f);
        }
#endif
    }
}
