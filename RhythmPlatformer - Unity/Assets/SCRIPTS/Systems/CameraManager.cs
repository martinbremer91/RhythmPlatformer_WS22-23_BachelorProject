using System;
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

        private float _maxY;
        private float _minY;
        private float _minX;
        private float _maxX;

        private Vector2 _camSize;

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

                _camSize.y = _cam.orthographicSize;
                _camSize.x = _camSize.y * _cam.aspect;
            }

            _currentNW = 
                GetClosestNode(_camNodes.Where(n => n.Position.x <= position.x && n.Position.y >= position.y));
            _currentNE = 
                GetClosestNode(_camNodes.Where(n => n.Position.x >= position.x && n.Position.y >= position.y));
            _currentSW = 
                GetClosestNode(_camNodes.Where(n => n.Position.x <= position.x && n.Position.y <= position.y));
            _currentSE = 
                GetClosestNode(_camNodes.Where(n => n.Position.x >= position.x && n.Position.y <= position.y));

            _maxY = Mathf.Max(_currentNW.y, _currentNE.y);
            _minY = Mathf.Min(_currentSW.y, _currentSE.y);
            _minX = Mathf.Min(_currentNW.x, _currentSW.x);
            _maxX = Mathf.Max(_currentNE.x, _currentSE.x);

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

            Vector3 targetPos = _characterTf.position;
            Vector3 position = transform.position;

            if (targetPos.x + _camSize.x > _maxX)
                targetPos.x = _maxX - _camSize.x;
            else if (targetPos.x - _camSize.x < _minX)
                targetPos.x = _minX + _camSize.x;

            if (targetPos.y + _camSize.y > _maxY)
                targetPos.y = _maxY - _camSize.y;
            else if (targetPos.y - _camSize.y < _minY)
                targetPos.y = _minY + _camSize.y;
            
            position = 
                Vector3.SmoothDamp(position, new Vector3(targetPos.x, targetPos.y, position.z), 
                    ref _velocity, _smoothTime, _maxSpeed);
            
            transform.position = position;
            _lastFramePos = position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.activeObject != gameObject)
                return;

            var pos = transform.position;
            Vector2 position = pos;
            
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
            
            Gizmos.color = Color.magenta;
            
            Gizmos.DrawWireSphere(new Vector3(pos.x, _maxY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(pos.x, _minY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_minX, pos.y, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_maxX, pos.y, 0), .5f);
        }
#endif
    }
}
