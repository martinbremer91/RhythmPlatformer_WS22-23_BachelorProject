using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces_and_Enums;
using Structs;
using UnityEngine;
using Utility_Scripts;
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
        #region REFERENCES

        [SerializeField] private CameraManager _complementaryCameraManager;

        [SerializeField] private Camera _cam;
        [SerializeField] private CameraConfigs _camConfigs;
        private TextAsset _camBoundsData;

        private CharacterStateController _characterStateController;
        private Transform _characterTransform;

        #endregion

        #region VARIABLES

        public UpdateType UpdateType => UpdateType.GamePlay;

        private Vector3 _characterPos;
        private Vector2 _characterMovementBoundaries;
        private bool _characterInMovementBoundaries;

        private CamNode[] _camNodes;
        private CamBoundsEdge[] _camBoundsEdgesHorizontal;
        private CamBoundsEdge[] _camBoundsEdgesVertical;

        private CamBoundsEdge frustrumNorthEdge;
        private CamBoundsEdge frustrumSouthEdge;
        private CamBoundsEdge frustrumEastEdge;
        private CamBoundsEdge frustrumWestEdge;

        private CamBoundsEdge _northEdge;
        private CamBoundsEdge _southEdge;
        private CamBoundsEdge _eastEdge;
        private CamBoundsEdge _westEdge;

        private readonly List<float> _nodeDistances = new();

        private CameraBounds _currentBounds;

        private Vector2 _targetPos;
        private Vector2 _camSize;

        private Vector3 _velocity;
        private float _smoothTime;
        private float _maxSpeed;
        private float _maxSize;
        private float _minSize;

        private bool _hasBounds;
        
        private Vector3 _currentSpawnPosition;
        private float _currentSpawnCamSize;
        private bool _isAssistant;
        
        #endregion

        #region INITIALIZATION

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterMovementBoundaries = _camConfigs.CharacterMovementBoundaries;
            _smoothTime = _camConfigs._smoothTime;
            _maxSpeed = _camConfigs.MaxSpeed;
            _maxSize = _camConfigs.MaxSize;
            _minSize = _camConfigs.MinSize;
            
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterTransform = _characterStateController.transform;

            _camBoundsData = in_gameStateManager.CameraBoundsData;

            GetCamNodesFromJson();
            InitializeNodeDistances();

            GetBoundEdgesFromNodes();

            _characterStateController.Respawn += JumpToSpawnPoint;
            _complementaryCameraManager.InitCameraManagerAssistant(this);

            GetFrustrumEdges();
            GetCurrentBoundsEdges();
            UpdateCurrentBounds();

            void GetBoundEdgesFromNodes() {
                int arrayLength = Mathf.RoundToInt(_camNodes.Length * .5f);
                CamBoundsEdge[] horizontalEdges = new CamBoundsEdge[arrayLength];
                CamBoundsEdge[] verticalEdges = new CamBoundsEdge[arrayLength];

                CamNode node;
                int neighborIndex = 0;
                for (int i = 0; i < _camNodes.Length; i++) {
                    node = _camNodes[neighborIndex];
                    bool horizontal = i % 2 == 0;
                    neighborIndex = horizontal ? node.HorizontalNeighborIndex :
                        node.VerticalNeighborIndex;

                    CamNode neighbor = _camNodes[neighborIndex];
                    int currentIndex = Mathf.FloorToInt(i * .5f);

                    if (horizontal)
                        horizontalEdges[currentIndex] = new CamBoundsEdge(true, node, neighbor);
                    else
                        verticalEdges[currentIndex] = new CamBoundsEdge(false, node, neighbor);

                    if (neighborIndex == 0)
                        break;
                }

                _camBoundsEdgesHorizontal = horizontalEdges;
                _camBoundsEdgesVertical = verticalEdges;
            }
        }
        
        private void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);
        
        private void InitializeNodeDistances()
        {
            for (int i = 0; i < _camNodes.Length; i++)
            {
                CamNode cn = _camNodes.FirstOrDefault(n => n.Index == i);
                _nodeDistances.Add(Vector3.Distance(cn.Position, transform.position));
            }
        }

        private void OnDisable()
        {
            if (!_isAssistant)
                _characterStateController.Respawn -= JumpToSpawnPoint;
        } 

        #endregion

        #region CAMERA MANAGER ASSISTANT / RESPAWN

        private void InitCameraManagerAssistant(CameraManager in_cameraManager)
        {
            Transform tf = transform;
            _isAssistant = true;
            tf.position = in_cameraManager.transform.position;
            _characterMovementBoundaries = in_cameraManager._characterMovementBoundaries;
            _camNodes = in_cameraManager._camNodes;
            _smoothTime = .1f;
            _maxSpeed = 1000;
            _maxSize = in_cameraManager._maxSize;
            _minSize = in_cameraManager._minSize;
            
            InitializeNodeDistances();
            _camBoundsEdgesHorizontal = in_cameraManager._camBoundsEdgesHorizontal;
            _camBoundsEdgesVertical = in_cameraManager._camBoundsEdgesVertical;
            UpdateOnRespawnPosAndSize(in_cameraManager);
        }

        public void OnSetCheckpoint(Vector3 in_spawnPos)
        {
            _complementaryCameraManager.gameObject.SetActive(true);
            _currentSpawnPosition =
                new Vector3(in_spawnPos.x, in_spawnPos.y, transform.position.z);
            _complementaryCameraManager.UpdateOnRespawnPosAndSize(this);
        }
        
        private void UpdateOnRespawnPosAndSize(CameraManager in_cameraManager)
        {
            _characterPos = in_cameraManager._currentSpawnPosition;
            _camSize.y = in_cameraManager._cam.orthographicSize;
            _camSize.x = _camSize.y * in_cameraManager._cam.aspect;
            
            UpdateSpawnValuesAndDeactivateAssistantAsync();
        }

        private async void UpdateSpawnValuesAndDeactivateAssistantAsync()
        {
            bool quitFunction = false;
            int timer = 0;

            SceneLoadManager.SceneUnloaded += QuitFunction;

            while (timer < 3000 && !CheckQuitFunction())
            {
                int deltaTimeMilliseconds = Mathf.RoundToInt(Time.deltaTime * 1000);

                _complementaryCameraManager._currentSpawnPosition = _currentSpawnPosition;
                _complementaryCameraManager._currentSpawnCamSize = _currentSpawnCamSize;

                timer += deltaTimeMilliseconds;
                await Task.Delay(deltaTimeMilliseconds);
            }

            if (CheckQuitFunction())
                return;

            gameObject.SetActive(false);

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }
        }

        private void JumpToSpawnPoint()
        {
            transform.position = _currentSpawnPosition;
            _cam.orthographicSize = _currentSpawnCamSize;
        }
        
        #endregion
        
        #region CUSTOM UPDATE / LATE UPDATE

        public void CustomUpdate()
        {
            Vector3 position = transform.position;

            if (!_isAssistant)
            {
                _camSize.y = _cam.orthographicSize;
                _camSize.x = _camSize.y * _cam.aspect;
                _characterPos = _characterTransform.position;
            }

            if (CheckCharacterInMovementBoundaries())
                return;

            Vector2 clampedCharacterPos = GetClampedTargetPos();
            Vector2 interolatedClampedTargetPos = 
                (Vector2)Vector3.SmoothDamp(position, new Vector3(clampedCharacterPos.x, clampedCharacterPos.y, position.z),
                    ref _velocity, _smoothTime, _maxSpeed);

            _targetPos = interolatedClampedTargetPos;
            
            GetFrustrumEdges();
            GetCurrentBoundsEdges();

            UpdateCurrentBounds();

            _hasBounds = true;

            bool CheckCharacterInMovementBoundaries()
            {
                _characterInMovementBoundaries =
                    _characterPos.x >= position.x - _characterMovementBoundaries.x && 
                    _characterPos.x <= position.x + _characterMovementBoundaries.x &&
                    _characterPos.y >= position.y - _characterMovementBoundaries.y && 
                    _characterPos.y <= position.y + _characterMovementBoundaries.y;
                return _characterInMovementBoundaries;
            }

            Vector2 GetClampedTargetPos() {
                Vector2 targetPos = new();

                if (_characterPos.x + _camSize.x > _currentBounds.MaxX)
                    targetPos.x = _currentBounds.MaxX - _camSize.x;
                else if (_characterPos.x - _camSize.x < _currentBounds.MinX)
                    targetPos.x = _currentBounds.MinX + _camSize.x;

                if (_characterPos.y + _camSize.y > _currentBounds.MaxY)
                    targetPos.y = _currentBounds.MaxY - _camSize.y;
                else if (_characterPos.y - _camSize.y < _currentBounds.MinY)
                    targetPos.y = _currentBounds.MinY + _camSize.y;

                return targetPos;
            }
        }

        private void GetFrustrumEdges() {
            Vector3 position = transform.position;

            Vector3 nw = position + Vector3.left * _camSize.x + Vector3.up * _camSize.y;
            Vector3 ne = position + Vector3.right * _camSize.x + Vector3.up * _camSize.y;
            Vector3 sw = position + Vector3.left * _camSize.x + Vector3.down * _camSize.y;
            Vector3 se = position + Vector3.right * _camSize.x + Vector3.down * _camSize.y;

            frustrumNorthEdge = new(true, nw, ne);
            frustrumSouthEdge = new(true, sw, se);
            frustrumEastEdge = new(false, ne, se);
            frustrumWestEdge = new(false, nw, sw);
        }

        private void GetCurrentBoundsEdges() {
            float maxY = float.MaxValue;
            float minY = float.MinValue;

            foreach (CamBoundsEdge edge in _camBoundsEdgesHorizontal) {
                bool aboveCam = edge.NodeAPos.y > _targetPos.y;
                CamBoundsEdge frustrumEdge = aboveCam ? frustrumNorthEdge : frustrumSouthEdge;

                float inverseLerpFrustrumEdgeA = (frustrumEdge.NodeAPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x);
                float inverseLerpFrustrumEdgeB = (frustrumEdge.NodeBPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x);

                if ((inverseLerpFrustrumEdgeA > 1 || inverseLerpFrustrumEdgeA < 0) &&
                    (inverseLerpFrustrumEdgeB > 1 || inverseLerpFrustrumEdgeB < 0))
                    continue;

                if (aboveCam) {
                    if (edge.NodeAPos.y < maxY) {
                        _northEdge = edge;
                        maxY = edge.NodeAPos.y;
                    }
                } else {
                    if (edge.NodeAPos.y > minY) {
                        _southEdge = edge;
                        minY = edge.NodeAPos.y;
                    }
                }
            }

            float maxX = float.MaxValue;
            float minX = float.MinValue;

            foreach (CamBoundsEdge edge in _camBoundsEdgesVertical) {
                bool rightOfCam = edge.NodeAPos.x > _targetPos.x;
                CamBoundsEdge frustrumEdge = rightOfCam ? frustrumEastEdge : frustrumWestEdge;

                float inverseLerpFrustrumEdgeA = (frustrumEdge.NodeAPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);
                float inverseLerpFrustrumEdgeB = (frustrumEdge.NodeBPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);

                if ((inverseLerpFrustrumEdgeA > 1 || inverseLerpFrustrumEdgeA < 0) &&
                    (inverseLerpFrustrumEdgeB > 1 || inverseLerpFrustrumEdgeB < 0))
                    continue;

                if (rightOfCam) {
                    if (edge.NodeAPos.x < maxX) {
                        _eastEdge = edge;
                        maxX = edge.NodeAPos.x;
                    }
                } else {
                    if (edge.NodeAPos.x > minX) {
                        _westEdge = edge;
                        minX = edge.NodeAPos.x;
                    }
                }
            }
        }

        private void UpdateCurrentBounds() {
            _currentBounds.MaxX = _eastEdge.NodeAPos.x;
            _currentBounds.MaxY = _northEdge.NodeAPos.x;
            _currentBounds.MinX = _westEdge.NodeAPos.y;
            _currentBounds.MinY = _southEdge.NodeAPos.y;
        }

        private void LateUpdate()
        {
            if (!_hasBounds || _characterInMovementBoundaries)
                return;

            _hasBounds = false;

            transform.position = _targetPos;
            SetCamSize();

            void SetCamSize()
            {
                float minVerticalSize =
                    Mathf.Min(_currentBounds.MaxY - _currentBounds.MinY, _currentBounds.MaxY - _currentBounds.MinY) * .5f;

                float minHorizontalSize =
                    Mathf.Min(_currentBounds.MaxX - _currentBounds.MinX, _currentBounds.MaxX - _currentBounds.MinX) * .5f;
                float verticalComplementForAspectRatio = minHorizontalSize / _cam.aspect;

                float targetSize =
                    Mathf.Clamp(Mathf.Min(minVerticalSize, verticalComplementForAspectRatio) - .1f, 
                        _minSize, _maxSize);

                float orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize, Time.deltaTime * 2);
                
                if (!_isAssistant)
                    _cam.orthographicSize = orthographicSize;
                else
                    _currentSpawnCamSize = orthographicSize;
            }
        }

        #endregion

        #region DEBUG / GIZMOS

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.activeObject != gameObject || _isAssistant)
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
            
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_characterPos, .1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(upperLeft, lowerLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerLeft, lowerRight);

            if (_camBoundsEdgesHorizontal == null || _camBoundsEdgesVertical == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPos, .1f);

            foreach (CamBoundsEdge edge in _camBoundsEdgesHorizontal) {
                Gizmos.color = edge.NodeAPos == _northEdge.NodeAPos ? Color.green : edge.NodeAPos == _southEdge.NodeAPos ? Color.blue : Color.black;
                Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
            }
            foreach (CamBoundsEdge edge in _camBoundsEdgesVertical) {
                Gizmos.color = edge.NodeAPos == _eastEdge.NodeAPos ? Color.red : edge.NodeAPos == _westEdge.NodeAPos ? Color.yellow : Color.black;
                Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
            }
        }
#endif

        #endregion
    }
}
