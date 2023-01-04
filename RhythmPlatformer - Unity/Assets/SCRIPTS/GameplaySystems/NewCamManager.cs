using Structs;
using UnityEditor;
using UnityEngine;
using Utility_Scripts;

public class NewCamManager : MonoBehaviour
{
    [SerializeField] private Transform _characterTransform;
    [SerializeField] private BoxCollider2D _characterCollider;
    [SerializeField] private Camera _cam;

    [SerializeField] private TextAsset _camBoundsData;

    private CamNode[] _camNodes;
    private CamBoundsEdge[] _camBoundsEdgesHorizontal;
    private CamBoundsEdge[] _camBoundsEdgesVertical;

    private Vector2 _characterPos;
    private Vector2 _characterSize;
    [SerializeField] private Vector2 _characterMovementBoundaries;
    private bool _characterWithinMovementBoundaries;
    private Vector3 _targetPos;
    private Vector2 _targetCamSize;

    [SerializeField] private float _minSize;
    [SerializeField] private float _maxSize;

    private CameraBounds _currentBounds;

    private CamBoundsEdge _characterNorthEdge;
    private CamBoundsEdge _characterSouthEdge;
    private CamBoundsEdge _characterEastEdge;
    private CamBoundsEdge _characterWestEdge;

    private CamBoundsEdge _characterNorthBoundary;
    private CamBoundsEdge _characterSouthBoundary;
    private CamBoundsEdge _characterEastBoundary;
    private CamBoundsEdge _characterWestBoundary;

    private Vector3 _velocity;
    [SerializeField] private float _smoothTime;
    [SerializeField] private float _maxSpeed;

    private void Start()
    {
        GetCamNodesFromJson();
        GetBoundEdgesFromNodes();

        void GetCamNodesFromJson() =>
            _camNodes = JsonArrayUtility.FromJson<CamNode>(_camBoundsData.text);

        _characterSize = _characterCollider.bounds.extents;

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

        GetCharacterEdges(currentCamSize);
        GetCurrentBoundaries();

        SetCamSize();

        Vector3 clampedTargetPos = GetClampedTargetPos();
        Vector3 interpolatedClampedTargetPos = Vector3.SmoothDamp(position,
            clampedTargetPos, ref _velocity, _smoothTime, _maxSpeed);

        _targetPos = interpolatedClampedTargetPos;

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
        transform.position = _targetPos;
    }

    private void GetCharacterEdges(Vector2 in_currentCamSize)
    {
        Bounds characterBounds = _characterCollider.bounds;
        float xCenter = characterBounds.center.x;
        float yCenter = characterBounds.center.y;

        Vector2 nwCharacter = new(xCenter - _characterSize.x, yCenter + _characterSize.y);
        Vector2 neCharacter = new(xCenter + _characterSize.x, yCenter + _characterSize.y);
        Vector2 swCharacter = new(xCenter - _characterSize.x, yCenter - _characterSize.y);
        Vector2 seCharacter = new(xCenter + _characterSize.x, yCenter - _characterSize.y);

        _characterNorthEdge = new(true, nwCharacter, neCharacter);
        _characterSouthEdge = new(true, swCharacter, seCharacter);
        _characterEastEdge = new(false, neCharacter, seCharacter);
        _characterWestEdge = new(false, nwCharacter, swCharacter);
    }

    private Vector3 GetClampedTargetPos()
    {
        Vector3 pos = new(_characterPos.x, _characterPos.y, transform.position.z);

        if (_characterPos.x + _targetCamSize.x > _currentBounds.MaxX)
            pos.x = _currentBounds.MaxX - _targetCamSize.x;
        else if (_characterPos.x - _targetCamSize.x < _currentBounds.MinX)
            pos.x = _currentBounds.MinX + _targetCamSize.x;

        if (_characterPos.y + _targetCamSize.y > _currentBounds.MaxY)
            pos.y = _currentBounds.MaxY - _targetCamSize.y;
        else if (_characterPos.y - _targetCamSize.y < _currentBounds.MinY)
            pos.y = _currentBounds.MinY + _targetCamSize.y;

        return pos;
    }

    private void GetCurrentBoundaries()
    {
        GetBoundaries(false, _camBoundsEdgesHorizontal);
        GetBoundaries(true, _camBoundsEdgesVertical);

        UpdateCurrentBounds();

        void GetBoundaries(bool in_vertical, CamBoundsEdge[] in_boundaryEdges) {
            float max = float.MaxValue;
            float min = float.MinValue;

            Vector2 posToCheck = _characterTransform.position;

            foreach (CamBoundsEdge edge in in_boundaryEdges) {
                bool edgeBeyondPos = 
                    !in_vertical ? edge.NodeAPos.y > posToCheck.y : edge.NodeAPos.x > posToCheck.x;
                
                CamBoundsEdge edgeToCheck = 
                    !in_vertical ? 
                        (edgeBeyondPos ? _characterNorthEdge : _characterSouthEdge) :
                        (edgeBeyondPos ? _characterEastEdge : _characterWestEdge);

                float inverseLerpEdgeA =
                    !in_vertical ? 
                        (edgeToCheck.NodeAPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x) :
                        (edgeToCheck.NodeAPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);
                float inverseLerpEdgeB =
                    !in_vertical ? 
                        (edgeToCheck.NodeBPos.x - edge.NodeAPos.x) / (edge.NodeBPos.x - edge.NodeAPos.x) : 
                        (edgeToCheck.NodeBPos.y - edge.NodeAPos.y) / (edge.NodeBPos.y - edge.NodeAPos.y);

                if ((inverseLerpEdgeA > 1 || inverseLerpEdgeA < 0) && (inverseLerpEdgeB > 1 || inverseLerpEdgeB < 0))
                    continue;

                float edgeValue = !in_vertical ? edge.NodeAPos.y : edge.NodeAPos.x;

                if (edgeBeyondPos) {
                    if (edgeValue < max) {
                        if (!in_vertical)
                            _characterNorthBoundary = edge;
                        else
                            _characterEastBoundary = edge;
                        
                        max = edgeValue;
                    }
                } else {
                    if (edgeValue > min) {
                        if (!in_vertical)
                            _characterSouthBoundary = edge;
                        else
                           _characterWestBoundary = edge;

                        min = edgeValue;
                    }
                }
            }
        }

        void UpdateCurrentBounds() {
            _currentBounds.MaxY = _characterNorthBoundary.NodeAPos.y - .1f;
            _currentBounds.MinY = _characterSouthBoundary.NodeAPos.y + .1f;
            _currentBounds.MaxX = _characterEastBoundary.NodeAPos.x - .1f;
            _currentBounds.MinX = _characterWestBoundary.NodeAPos.x + .1f;
        }
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
            bool north = edge.NodeAPos == _characterNorthBoundary.NodeAPos;
            bool south = edge.NodeAPos == _characterSouthBoundary.NodeAPos;

            Gizmos.color = north ? Color.green : south ? Color.blue : Color.black;
            Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
        }
        foreach (CamBoundsEdge edge in _camBoundsEdgesVertical)
        {
            bool east = edge.NodeAPos == _characterEastBoundary.NodeAPos;
            bool west = edge.NodeAPos == _characterWestBoundary.NodeAPos;

            Gizmos.color = east ? Color.red : west ? Color.yellow : Color.black;
            Gizmos.DrawLine(edge.NodeAPos, edge.NodeBPos);
        }
    }
}
