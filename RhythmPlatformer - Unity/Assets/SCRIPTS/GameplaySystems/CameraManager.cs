using System.Collections.Generic;
using System.Linq;
using Interfaces_and_Enums;
using Structs;
using UnityEngine;
using Utility_Scripts;
using System.Collections;
using Gameplay;
using GlobalSystems;
using Scriptable_Object_Scripts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameplaySystems
{
    public class CameraManager : MonoBehaviour, IInit<GameStateManager>, IUpdatable
    {
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        [SerializeField] private Camera _cam;
        [SerializeField] private CameraConfigs _camConfigs;
        private TextAsset _camBoundsData;

        private CharacterStateController _characterStateController;
        private Transform _characterTf;
        private Vector3 _characterPos;
        private float _characterPosYOffset;
        private Vector2 _characterMovementBoundaries;
        private float _lookAheadShiftDistance;
        private int _lookAheadDirection;
        private bool _getCharacterIntoMovementBoundaries;

        private CamNode[] _camNodes;
        
        private readonly List<float> _nodeDistances = new();

        private CameraBounds _characterPosBounds;
        private CameraBounds _northBounds;
        private CameraBounds _westBounds;
        private CameraBounds _southBounds;
        private CameraBounds _eastBounds;

        private Vector2 _camSize;

        private Vector3 _velocity;
        private float _smoothTime;
        private float _maxSpeed;
        private float _maxSize;
        private float _minSize;

        private bool _hasBounds;
#if UNITY_EDITOR
        private Vector3 _debugMovementBoundariesVector;
#endif

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterMovementBoundaries = _camConfigs.CharacterMovementBoundaries;
            _lookAheadShiftDistance = _camConfigs.LookAheadShiftDistance;
            _smoothTime = _camConfigs._smoothTime;
            _maxSpeed = _camConfigs.MaxSpeed;
            _maxSize = _camConfigs.MaxSize;
            _minSize = _camConfigs.MinSize;
            
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterTf = _characterStateController.transform;

            _characterPosYOffset = _characterTf.GetComponent<BoxCollider2D>().bounds.size.y * .5f;

            _camBoundsData = in_gameStateManager.CameraBoundsData;
            GetCamNodesFromJson();

            for (int i = 0; i < _camNodes.Length; i++)
            {
                CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                _nodeDistances.Add(Vector3.Distance(cn.Position, transform.position));
            }
        }

        private void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);

        public void CustomUpdate()
        {
            Vector3 position = transform.position;
            _camSize.y = _cam.orthographicSize;
            _camSize.x = _camSize.y * _cam.aspect;
            
            _characterPos = _characterTf.position + Vector3.up * _characterPosYOffset;
            _lookAheadDirection = _characterStateController.LookAheadDirection;

            bool camOutOfBounds;
            UpdateCurrentBounds();

            if (!camOutOfBounds)
                CheckCharacterInMovementBoundaries();

            _hasBounds = true;

            void UpdateCurrentBounds()
            {
                bool northPosInBounds, westPosInBounds, southPosInBounds, eastPosInBounds;

                _characterPosBounds = GetCamBounds(_characterPosBounds, _characterPos, true, true);

                Vector3 northPos = new Vector3(position.x, position.y + _camSize.y, 0);
                if (northPosInBounds = CheckPointInBounds(northPos, _characterPosBounds))
                    _northBounds = GetCamBounds(_northBounds, northPos, true, false);
            
                Vector3 westPos = new Vector3(position.x - _camSize.x, position.y, 0);
                if(westPosInBounds = CheckPointInBounds(westPos, _characterPosBounds))
                    _westBounds = GetCamBounds(_westBounds, westPos, false, true);
            
                Vector3 southPos = new Vector3(position.x, position.y - _camSize.y, 0);
                if (southPosInBounds = CheckPointInBounds(southPos, _characterPosBounds))
                    _southBounds = GetCamBounds(_southBounds, southPos, true, false);
            
                Vector3 eastPos = new Vector3(position.x + _camSize.x, position.y, 0);
                if(eastPosInBounds = CheckPointInBounds(eastPos, _characterPosBounds))
                    _eastBounds = GetCamBounds(_eastBounds, eastPos, false, true);

                camOutOfBounds = _lookAheadDirection == 0 && 
                    (!northPosInBounds || !westPosInBounds || !southPosInBounds || eastPosInBounds);

                if (camOutOfBounds)
                    _getCharacterIntoMovementBoundaries = false;
            }

            void CheckCharacterInMovementBoundaries()
            {
                float maxMinX = _characterMovementBoundaries.x;
                float maxY = _lookAheadDirection == 0 ? _characterMovementBoundaries.y :
                    _lookAheadDirection < 0 ? _characterMovementBoundaries.y + _lookAheadShiftDistance :
                    _characterMovementBoundaries.y - _lookAheadShiftDistance;
                float minY = _lookAheadDirection == 0 ? -_characterMovementBoundaries.y :
                    _lookAheadDirection < 0 ? -_characterMovementBoundaries.y + _lookAheadShiftDistance :
                    -_characterMovementBoundaries.y - _lookAheadShiftDistance;
#if UNITY_EDITOR
                _debugMovementBoundariesVector = new Vector3(maxMinX, maxY, minY);
#endif
                _getCharacterIntoMovementBoundaries =
                    _characterPos.x >= position.x - maxMinX && 
                    _characterPos.x <= position.x + maxMinX &&
                    _characterPos.y >= position.y + minY && 
                    _characterPos.y <= position.y + maxY;
            }

            CameraBounds GetCamBounds(CameraBounds in_default, Vector3 in_pos, bool in_getX, bool in_getY)
            {
                CameraBounds camBounds = new CameraBounds();
                
                for (int i = 0; i < _camNodes.Length; i++)
                {
                    CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                    _nodeDistances[i] = Vector3.Distance(cn.Position, in_pos);
                }

                CamNode[] nwNodes = 
                    _camNodes.Where(n => n.Position.x <= in_pos.x && n.Position.y >= in_pos.y).ToArray();
                CamNode[] neNodes = 
                    _camNodes.Where(n => n.Position.x >= in_pos.x && n.Position.y >= in_pos.y).ToArray();
                CamNode[] swNodes = 
                    _camNodes.Where(n => n.Position.x <= in_pos.x && n.Position.y <= in_pos.y).ToArray();
                CamNode[] seNodes = 
                    _camNodes.Where(n => n.Position.x >= in_pos.x && n.Position.y <= in_pos.y).ToArray();

                if (!nwNodes.Any() || !neNodes.Any() || !swNodes.Any() || !seNodes.Any())
                    return in_default;
                
                Vector2 currentNW = GetClosestNode(nwNodes);
                Vector2 currentNE = GetClosestNode(neNodes);
                Vector2 currentSW = GetClosestNode(swNodes);
                Vector2 currentSE = GetClosestNode(seNodes);
                
                if (in_getY)
                {
                    camBounds.MaxY = Mathf.Max(currentNW.y, currentNE.y);
                    camBounds.MinY = Mathf.Min(currentSW.y, currentSE.y);
                }
                if (in_getX)
                {
                    camBounds.MinX = Mathf.Min(currentNW.x, currentSW.x);
                    camBounds.MaxX = Mathf.Max(currentNE.x, currentSE.x);
                }

                return camBounds;
            }
            
            Vector2 GetClosestNode(IEnumerable<CamNode> nodes)
            {
                return nodes.Aggregate((minD, n) => 
                        _nodeDistances[n.Index] < _nodeDistances[minD.Index] ? n : minD).Position;
            }

            bool CheckPointInBounds(Vector3 in_point, CameraBounds in_bounds)
            {
                return in_point.x > in_bounds.MinX && in_point.x < in_bounds.MaxX && in_point.y > in_bounds.MinY &&
                       in_point.y < in_bounds.MaxY;
            }
        }

        private void LateUpdate()
        {
            if (!_hasBounds || _getCharacterIntoMovementBoundaries)
                return;

            _hasBounds = false;

            Vector3 position = transform.position;
            Vector3 targetPos = _characterPos;
            targetPos.y = _lookAheadDirection == 0 ? targetPos.y :
                _lookAheadDirection > 0 ? targetPos.y + _lookAheadShiftDistance : targetPos.y - _lookAheadShiftDistance;

            GetClampedTargetPos();
            
            position = 
                Vector3.SmoothDamp(position, new Vector3(targetPos.x, targetPos.y, position.z), 
                    ref _velocity, _smoothTime, _maxSpeed);
            transform.position = position;

            SetCamSize();

            void GetClampedTargetPos()
            {
                if (targetPos.x + _camSize.x > _characterPosBounds.MaxX)
                    targetPos.x = _characterPosBounds.MaxX - _camSize.x;
                else if (targetPos.x - _camSize.x < _characterPosBounds.MinX)
                    targetPos.x = _characterPosBounds.MinX + _camSize.x;

                int lookUpOffset = _lookAheadDirection > 0 ? 100 : 0;
                int lookDownOffset = _lookAheadDirection < 0 ? 100 : 0;
                
                if (targetPos.y + _camSize.y > _characterPosBounds.MaxY + lookUpOffset)
                    targetPos.y = _characterPosBounds.MaxY - _camSize.y;
                if (targetPos.y - _camSize.y < _characterPosBounds.MinY - lookDownOffset)
                    targetPos.y = _characterPosBounds.MinY + _camSize.y;
            }

            void SetCamSize()
            {
                float minVerticalSize =
                    Mathf.Min(_westBounds.MaxY - _westBounds.MinY, _eastBounds.MaxY - _eastBounds.MinY) * .5f;

                float minHorizontalSize =
                    Mathf.Min(_northBounds.MaxX - _northBounds.MinX, _southBounds.MaxX - _southBounds.MinX) * .5f;
                float verticalComplementForAspectRatio = minHorizontalSize / _cam.aspect;

                float targetSize =
                    Mathf.Clamp(Mathf.Min(minVerticalSize, verticalComplementForAspectRatio) - .05f, 
                        _minSize, _maxSize);
            
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, Time.deltaTime * 2);
            }
        }

        #region DEBUG / GIZMOS

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.activeObject != gameObject)
                return;

            var pos = transform.position;
            Vector2 position = pos;
            
            Vector2 upperLeft = 
                position + new Vector2(-_debugMovementBoundariesVector.x, _debugMovementBoundariesVector.y);
            Vector2 lowerLeft = 
                position + new Vector2(-_debugMovementBoundariesVector.x, _debugMovementBoundariesVector.z);
            Vector2 upperRight = 
                position + new Vector2(_debugMovementBoundariesVector.x, _debugMovementBoundariesVector.y);
            Vector2 lowerRight =
                position + new Vector2(_debugMovementBoundariesVector.x, _debugMovementBoundariesVector.z);
            
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_characterPos, .1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(upperLeft, lowerLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerLeft, lowerRight);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(new Vector3(_characterPos.x, _characterPosBounds.MaxY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_characterPos.x, _characterPosBounds.MinY, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_characterPosBounds.MinX, _characterPos.y, 0), .5f);
            Gizmos.DrawWireSphere(new Vector3(_characterPosBounds.MaxX, _characterPos.y, 0), .5f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(_northBounds.MinX, position.y + _camSize.y, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(_northBounds.MaxX, position.y + _camSize.y, 0), .3f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(position.x - _camSize.x, _westBounds.MinY, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(position.x - _camSize.x, _westBounds.MaxY, 0), .3f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(_southBounds.MinX, position.y - _camSize.y, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(_southBounds.MaxX, position.y - _camSize.y, 0), .3f);
            
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(new Vector3(position.x + _camSize.x, _eastBounds.MinY, 0), .3f);
            Gizmos.DrawWireSphere(new Vector3(position.x + _camSize.x, _eastBounds.MaxY, 0), .3f);
        }
#endif

        #endregion
    }
}
