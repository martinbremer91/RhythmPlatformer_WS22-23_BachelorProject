using Structs;
using UnityEditor;
using UnityEngine;
using Utility_Scripts;

public class NewCamManager : MonoBehaviour
{
    [SerializeField] private Transform _characterTransform;
    [SerializeField] private BoxCollider2D _characterCollider;
    private Vector2 _characterSize;
    [SerializeField] private Camera _cam;

    [SerializeField] private TextAsset _camBoundsData;

    private CamNode[] _camNodes;
    private CamBoundsEdge[] _camBoundsEdgesHorizontal;
    private CamBoundsEdge[] _camBoundsEdgesVertical;

    private Vector2 _characterPos;
    [SerializeField] private Vector2 _characterMovementBoundaries;
    private bool _characterWithinMovementBoundaries;
    private Vector3 _targetPos;
    private Vector2 _targetCamSize;

    [SerializeField] private float _minSize;
    [SerializeField] private float _maxSize;

    private CameraBounds _currentBounds;

    private CamBoundsEdge _sizeNorthEdge;
    private CamBoundsEdge _sizeSouthEdge;
    private CamBoundsEdge _sizeEastEdge;
    private CamBoundsEdge _sizeWestEdge;

    private CamBoundsEdge _posNorthEdge;
    private CamBoundsEdge _posSouthEdge;
    private CamBoundsEdge _posEastEdge;
    private CamBoundsEdge _posWestEdge;

    private CamBoundsEdge _frustrumNorthEdge;
    private CamBoundsEdge _frustrumSouthEdge;
    private CamBoundsEdge _frustrumEastEdge;
    private CamBoundsEdge _frustrumWestEdge;

    private Vector3 _velocity;
    [SerializeField] private float _smoothTime;
    [SerializeField] private float _maxSpeed;

    private void Start()
    {
        GetCamNodesFromJson();
        GetBoundEdgesFromNodes();

        void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);

        _characterSize = new(_characterCollider.size.x, _characterCollider.size.y);

        void GetBoundEdgesFromNodes()
        {
            int arrayLength = Mathf.RoundToInt(_camNodes.Length * .5f);
            CamBoundsEdge[] horizontalEdges = new CamBoundsEdge[arrayLength];
            CamBoundsEdge[] verticalEdges = new CamBoundsEdge[arrayLength];

            CamNode node;
            int neighborIndex = 0;
            for (int i = 0; i < _camNodes.Length; i++)
            {
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

    void Update()
    {
        Vector3 position = transform.position;

        Vector2 currentCamSize = new();
        currentCamSize.y = _cam.orthographicSize;
        currentCamSize.x = currentCamSize.y * _cam.aspect;

        _characterPos = _characterTransform.position;

        if (CheckCharacterInMovementBoundaries())
            return;

        GetFrustrumEdges(currentCamSize);
        GetCurrentBoundsEdges();
        UpdateCurrentBounds();

        SetCamSize();

        Vector3 clampedTargetPos = GetClampedTargetPos();
        Vector3 interpolatedClampedTargetPos = Vector3.SmoothDamp(position,
            clampedTargetPos, ref _velocity, _smoothTime, _maxSpeed);

        _targetPos = interpolatedClampedTargetPos;

        // check if cam currently fits in target pos
            // if so, move cam
            // else, check if character is within frustrum
                // if so, return
                // else, move
        // set (persistent target cam size)

        bool CheckCharacterInMovementBoundaries()
        {
            _characterWithinMovementBoundaries =
                _characterPos.x >= position.x - _characterMovementBoundaries.x &&
                _characterPos.x <= position.x + _characterMovementBoundaries.x &&
                _characterPos.y >= position.y - _characterMovementBoundaries.y &&
                _characterPos.y <= position.y + _characterMovementBoundaries.y;
            return _characterWithinMovementBoundaries;
        }
    }

    private void LateUpdate()
    {
        // interpolate pos and size
        transform.position = _targetPos;
    }
    
    private Vector3 GetClampedTargetPos()
    {
        Vector3 pos = new(_characterPos.x, _characterPos.y, transform.position.z);

        if (_characterPos.x + _targetCamSize.x > _currentBounds.MaxX)
            pos.x = _currentBounds.MaxX - _targetCamSize.x - .1f;
        else if (_characterPos.x - _targetCamSize.x < _currentBounds.MinX)
            pos.x = _currentBounds.MinX + _targetCamSize.x + .1f;

        if (_characterPos.y + _targetCamSize.y > _currentBounds.MaxY)
            pos.y = _currentBounds.MaxY - _targetCamSize.y - .1f;
        else if (_characterPos.y - _targetCamSize.y < _currentBounds.MinY)
            pos.y = _currentBounds.MinY + _targetCamSize.y + .1f;

        return pos;
    }

    private void GetFrustrumEdges(Vector2 in_currentCamSize)
    {
        Vector2 nw = (Vector2)_targetPos + Vector2.left * in_currentCamSize.x + Vector2.up * in_currentCamSize.y;
        Vector2 ne = (Vector2)_targetPos + Vector2.right * in_currentCamSize.x + Vector2.up * in_currentCamSize.y;
        Vector2 sw = (Vector2)_targetPos + Vector2.left * in_currentCamSize.x + Vector2.down * in_currentCamSize.y;
        Vector2 se = (Vector2)_targetPos + Vector2.right * in_currentCamSize.x + Vector2.down * in_currentCamSize.y;

        _frustrumNorthEdge = new(true, nw, ne);
        _frustrumSouthEdge = new(true, sw, se);
        _frustrumEastEdge = new(false, ne, se);
        _frustrumWestEdge = new(false, nw, sw);
    }

    private void GetCurrentBoundsEdges()
    {
        float maxY = float.MaxValue;
        float minY = float.MinValue;

        foreach (CamBoundsEdge edge in _camBoundsEdgesHorizontal)
        {
            bool aboveCam = edge.NodeAPos.y > _targetPos.y;
            CamBoundsEdge frustrumEdge = aboveCam ? _frustrumNorthEdge : _frustrumSouthEdge;

            float inverseLerpFrustrumEdgeA = (frustrumEdge.NodeAPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x);
            float inverseLerpFrustrumEdgeB = (frustrumEdge.NodeBPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x);

            if ((inverseLerpFrustrumEdgeA > 1 || inverseLerpFrustrumEdgeA < 0) &&
                (inverseLerpFrustrumEdgeB > 1 || inverseLerpFrustrumEdgeB < 0))
                continue;

            if (aboveCam)
            {
                if (edge.NodeAPos.y < maxY)
                {
                    _northEdge = edge;
                    maxY = edge.NodeAPos.y;
                }
            }
            else
            {
                if (edge.NodeAPos.y > minY)
                {
                    _southEdge = edge;
                    minY = edge.NodeAPos.y;
                }
            }
        }

        float maxX = float.MaxValue;
        float minX = float.MinValue;

        foreach (CamBoundsEdge edge in _camBoundsEdgesVertical)
        {
            bool rightOfCam = edge.NodeAPos.x > _targetPos.x;
            CamBoundsEdge frustrumEdge = rightOfCam ? _frustrumEastEdge : _frustrumWestEdge;

            float inverseLerpFrustrumEdgeA = (frustrumEdge.NodeAPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);
            float inverseLerpFrustrumEdgeB = (frustrumEdge.NodeBPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);

            if ((inverseLerpFrustrumEdgeA > 1 || inverseLerpFrustrumEdgeA < 0) &&
                (inverseLerpFrustrumEdgeB > 1 || inverseLerpFrustrumEdgeB < 0))
                continue;

            if (rightOfCam)
            {
                if (edge.NodeAPos.x < maxX)
                {
                    _eastEdge = edge;
                    maxX = edge.NodeAPos.x - .1f;
                }
            }
            else
            {
                if (edge.NodeAPos.x > minX)
                {
                    _westEdge = edge;
                    minX = edge.NodeAPos.x + .1f;
                }
            }
        }
    }

    private void UpdateCurrentBounds()
    {
        _currentBounds.MaxY = _northEdge.NodeAPos.y;
        _currentBounds.MinY = _southEdge.NodeAPos.y;
        _currentBounds.MaxX = _eastEdge.NodeAPos.x;
        _currentBounds.MinX = _westEdge.NodeAPos.x;
    }

    private void SetCamSize()
    {
        float minVerticalSize =
            (_currentBounds.MaxY - _currentBounds.MinY) * .5f;

        float minHorizontalSize =
            (_currentBounds.MaxX - _currentBounds.MinX) * .5f;
        float verticalComplementForAspectRatio = minHorizontalSize / _cam.aspect;

        float clampedOrthographicSize =
            Mathf.Clamp(Mathf.Min(minVerticalSize, verticalComplementForAspectRatio) - .1f,
                _minSize, _maxSize);

        float interpolatedClampedOrthographicSize = Mathf.Lerp(_cam.orthographicSize, clampedOrthographicSize, Time.deltaTime * 2);

        _targetCamSize = new(interpolatedClampedOrthographicSize * _cam.aspect - .1f, interpolatedClampedOrthographicSize);
        _cam.orthographicSize = interpolatedClampedOrthographicSize;
    }

    private void OnDrawGizmos()
    {
        if (Selection.activeObject != gameObject)
            return;

        if (_camBoundsEdgesHorizontal == null || _camBoundsEdgesVertical == null)
            return;

        Vector2 position = transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_targetPos, .1f);

        Vector2 upperLeft =
                position + new Vector2(-_characterMovementBoundaries.x, _characterMovementBoundaries.y);
        Vector2 lowerLeft =
            position + new Vector2(-_characterMovementBoundaries.x, -_characterMovementBoundaries.y);
        Vector2 upperRight =
            position + new Vector2(_characterMovementBoundaries.x, _characterMovementBoundaries.y);
        Vector2 lowerRight =
            position + new Vector2(_characterMovementBoundaries.x, -_characterMovementBoundaries.y);

        Gizmos.color = _characterWithinMovementBoundaries ? Color.green : Color.red;
        Gizmos.DrawLine(upperLeft, lowerLeft);
        Gizmos.DrawLine(upperLeft, upperRight);
        Gizmos.DrawLine(upperRight, lowerRight);
        Gizmos.DrawLine(lowerLeft, lowerRight);

        Gizmos.DrawWireSphere(new Vector3(_currentBounds.MaxX, position.y, 0), .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(_currentBounds.MinX, position.y, 0), .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(position.x, _currentBounds.MaxY, 0), .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(position.x, _currentBounds.MinY, 0), .1f);

        foreach (CamBoundsEdge edge in _camBoundsEdgesHorizontal)
        {
            Gizmos.color = edge.NodeAPos == _northEdge.NodeAPos ? Color.green : edge.NodeAPos == _southEdge.NodeAPos ? Color.blue : Color.black;
            Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
        }
        foreach (CamBoundsEdge edge in _camBoundsEdgesVertical)
        {
            Gizmos.color = edge.NodeAPos == _eastEdge.NodeAPos ? Color.red : edge.NodeAPos == _westEdge.NodeAPos ? Color.yellow : Color.black;
            Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
        }
    }
}
