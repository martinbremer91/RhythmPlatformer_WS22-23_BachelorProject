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
        
        private readonly List<float> _nodeDistances = new();

        private CameraBounds _center;
        private CameraBounds _north;
        private CameraBounds _west;
        private CameraBounds _south;
        private CameraBounds _east;

        private Vector2 _camSize;

        private bool _characterInBoundaries;
        private Vector3 _velocity;
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
            Vector3 position = transform.position;
            
            Vector3 characterPosition = _characterTf.position;
            _characterInBoundaries =
                characterPosition.x >= position.x - _characterMovementBoundaries.x && 
                characterPosition.x <= position.x + _characterMovementBoundaries.x &&
                characterPosition.y >= position.y - _characterMovementBoundaries.y && 
                characterPosition.y <= position.y + _characterMovementBoundaries.y;

            _camSize.y = _cam.orthographicSize;
            _camSize.x = _camSize.y * _cam.aspect;

            _center = GetCamBounds(position);
            _north = GetCamBounds(new Vector3(position.x, position.y + _camSize.y, 0));
            _west = GetCamBounds(new Vector3(position.x - _camSize.x, position.y, 0));
            _south = GetCamBounds(new Vector3(position.x, position.y - _camSize.y, 0));
            _east = GetCamBounds(new Vector3(position.x + _camSize.x, position.y, 0));

            CameraBounds GetCamBounds(Vector3 in_pos)
            {
                CameraBounds camBounds = new CameraBounds();
                
                for (int i = 0; i < _camNodes.Length; i++)
                {
                    CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                    _nodeDistances[i] = Vector3.Distance(cn.Position, in_pos);
                }

                // TODO: these variables can probably be local, since the only thing that uses them out of scope
                // TODO: is OnDrawGizmos
                camBounds.CurrentNW =
                    GetClosestNode(_camNodes.Where(n => n.Position.x <= in_pos.x && n.Position.y >= in_pos.y));
                camBounds.CurrentNE = 
                    GetClosestNode(_camNodes.Where(n => n.Position.x >= in_pos.x && n.Position.y >= in_pos.y));
                camBounds.CurrentSW = 
                    GetClosestNode(_camNodes.Where(n => n.Position.x <= in_pos.x && n.Position.y <= in_pos.y));
                camBounds.CurrentSE = 
                    GetClosestNode(_camNodes.Where(n => n.Position.x >= in_pos.x && n.Position.y <= in_pos.y));

                // TODO: only _center needs all these values. E.g. _west only needs MaxY and MinY
                camBounds.MaxY = Mathf.Max(camBounds.CurrentNW.y, camBounds.CurrentNE.y);
                camBounds.MinY = Mathf.Min(camBounds.CurrentSW.y, camBounds.CurrentSE.y);
                camBounds.MinX = Mathf.Min(camBounds.CurrentNW.x, camBounds.CurrentSW.x);
                camBounds.MaxX = Mathf.Max(camBounds.CurrentNE.x, camBounds.CurrentSE.x);

                return camBounds;
            }
            
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

            if (targetPos.x + _camSize.x > _center.MaxX)
                targetPos.x = _center.MaxX - _camSize.x;
            else if (targetPos.x - _camSize.x < _center.MinX)
                targetPos.x = _center.MinX + _camSize.x;

            if (targetPos.y + _camSize.y > _center.MaxY)
                targetPos.y = _center.MaxY - _camSize.y;
            else if (targetPos.y - _camSize.y < _center.MinY)
                targetPos.y = _center.MinY + _camSize.y;
            
            position = 
                Vector3.SmoothDamp(position, new Vector3(targetPos.x, targetPos.y, position.z), 
                    ref _velocity, _smoothTime, _maxSpeed);
            
            transform.position = position;

            float minVertSize = Mathf.Min(_west.MaxY - _west.MinY, _east.MaxY - _east.MinY) * .5f;
            
            float minHorSize = Mathf.Min(_north.MaxX - _north.MinX, _south.MaxX - _south.MinX) * .5f;
            float vertComplement = minHorSize / _cam.aspect;

            float targetSize = Mathf.Min(minVertSize, vertComplement) - .5f;
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, Time.deltaTime * 2);
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
            
            Gizmos.DrawWireSphere(_center.CurrentNW, .5f);
            Gizmos.DrawWireSphere(_center.CurrentNE, .5f);
            Gizmos.DrawWireSphere(_center.CurrentSW, .5f);
            Gizmos.DrawWireSphere(_center.CurrentSE, .5f);
            
            Gizmos.color = Color.magenta;
            
            Gizmos.DrawWireSphere(new Vector3(pos.x, _center.MaxY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(pos.x, _center.MinY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_center.MinX, pos.y, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_center.MaxX, pos.y, 0), .5f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(_north.MinX, position.y + _camSize.y, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(_north.MaxX, position.y + _camSize.y, 0), .3f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(position.x - _camSize.x, _west.MinY, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(position.x - _camSize.x, _west.MaxY, 0), .3f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(_south.MinX, position.y - _camSize.y, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(_south.MaxX, position.y - _camSize.y, 0), .3f);
            
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(new Vector3(position.x + _camSize.x, _east.MinY, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(position.x + _camSize.x, _east.MaxY, 0), .3f);
        }
#endif
    }
}
