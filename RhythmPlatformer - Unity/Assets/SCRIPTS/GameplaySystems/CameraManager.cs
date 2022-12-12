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

            _characterStateController.Respawn += JumpToSpawnPoint;
            _complementaryCameraManager.InitCameraManagerAssistant(this);
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

            bool camOutOfBounds;
            UpdateCurrentBounds();

            if (!camOutOfBounds)
                CheckCharacterInMovementBoundaries();

            _hasBounds = true;

            void UpdateCurrentBounds()
            {
                _characterPosBounds = GetCamBounds(_characterPosBounds, _characterPos, true, true);

                Vector3 northPos = new Vector3(position.x, position.y + _camSize.y, 0);
                Vector3 westPos = new Vector3(position.x - _camSize.x, position.y, 0);
                Vector3 southPos = new Vector3(position.x, position.y - _camSize.y, 0);
                Vector3 eastPos = new Vector3(position.x + _camSize.x, position.y, 0);
                
                bool northPosInBounds = CheckPointInBounds(northPos, _characterPosBounds);
                bool westPosInBounds = CheckPointInBounds(westPos, _characterPosBounds);
                bool southPosInBounds = CheckPointInBounds(southPos, _characterPosBounds);
                bool eastPosInBounds = CheckPointInBounds(eastPos, _characterPosBounds);
                
                if (northPosInBounds)
                    _northBounds = GetCamBounds(_northBounds, northPos, true, false);
                if(westPosInBounds)
                    _westBounds = GetCamBounds(_westBounds, westPos, false, true);
                if (southPosInBounds)
                    _southBounds = GetCamBounds(_southBounds, southPos, true, false);
                if(eastPosInBounds)
                    _eastBounds = GetCamBounds(_eastBounds, eastPos, false, true);

                camOutOfBounds =
                    !northPosInBounds || !westPosInBounds || !southPosInBounds || eastPosInBounds;

                if (camOutOfBounds)
                    _getCharacterIntoMovementBoundaries = false;
            }

            void CheckCharacterInMovementBoundaries()
            {
                _getCharacterIntoMovementBoundaries =
                    _characterPos.x >= position.x - _characterMovementBoundaries.x && 
                    _characterPos.x <= position.x + _characterMovementBoundaries.x &&
                    _characterPos.y >= position.y - _characterMovementBoundaries.y && 
                    _characterPos.y <= position.y + _characterMovementBoundaries.y;
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

            GetClampedTargetPos();
            
            position = 
                Vector3.SmoothDamp(position, new Vector3(_characterPos.x, _characterPos.y, position.z), 
                    ref _velocity, _smoothTime, _maxSpeed);
            
            transform.position = position;

            if (_isAssistant)
                _currentSpawnPosition = position;

            SetCamSize();

            void GetClampedTargetPos()
            {
                if (_characterPos.x + _camSize.x > _characterPosBounds.MaxX)
                    _characterPos.x = _characterPosBounds.MaxX - _camSize.x;
                else if (_characterPos.x - _camSize.x < _characterPosBounds.MinX)
                    _characterPos.x = _characterPosBounds.MinX + _camSize.x;
                
                if (_characterPos.y + _camSize.y > _characterPosBounds.MaxY)
                    _characterPos.y = _characterPosBounds.MaxY - _camSize.y;
                else if (_characterPos.y - _camSize.y < _characterPosBounds.MinY)
                    _characterPos.y = _characterPosBounds.MinY + _camSize.y;
            }

            void SetCamSize()
            {
                float minVerticalSize =
                    Mathf.Min(_westBounds.MaxY - _westBounds.MinY, _eastBounds.MaxY - _eastBounds.MinY) * .5f;

                float minHorizontalSize =
                    Mathf.Min(_northBounds.MaxX - _northBounds.MinX, _southBounds.MaxX - _southBounds.MinX) * .5f;
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
