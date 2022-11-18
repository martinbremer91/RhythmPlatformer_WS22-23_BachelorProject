using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public class CompanionFollow : MonoBehaviour, IUpdatable, IInit<GameStateManager>, IPhysicsPausable
{
    public UpdateType UpdateType => UpdateType.GamePlay;

    public Rigidbody2D PausableRigidbody => _rigidbody2D;
    public Vector2 Velocity => _velocity;

    private GameStateManager _gameStateManager;
    private Transform _characterTransform;
    [SerializeField] private Rigidbody2D _rigidbody2D;

    private Vector2 _characterOffset;

    private AnimationCurve _followAccelerationCurve;
    private AnimationCurve _followArcVelocityCurve;
    private Vector2 _followAccelerationCurveTracker;

    private float _followSpeedMax;
    private float _followSpeedMin;

    private float _followArcOffsetMax;

    private Vector2 _velocity;

    private float _maxSpeedDist;
    private float _minSpeedDist;

    private float _reactionDelay;
    private float _reactionTimer;
    private float _startFollowDist;
    private float _stopFollowDist;

    private bool _followingCharacter;

    private void OnEnable() => RegisterPhysicsPausable();
    private void OnDisable() => DeregisterPhysicsPausable();

    public void RegisterPhysicsPausable() {
        if (_gameStateManager && !_gameStateManager.PhysicsPausables.Contains(this))
            _gameStateManager.PhysicsPausables.Add(this);
    }

    public void DeregisterPhysicsPausable() =>
         _gameStateManager.PhysicsPausables.Remove(this);

    public void Init(GameStateManager in_gameStateManager) {
        _gameStateManager = in_gameStateManager;
        CompanionConfigs configs = in_gameStateManager.CompanionConfigs;

        _characterTransform = in_gameStateManager.CharacterStateController.transform;

        _followAccelerationCurve = configs.FollowAccelerationCurve;
        _followArcVelocityCurve = configs.FollowArcVelocityCurve;
        _characterOffset = configs.CharacterOffset;
        _reactionDelay = configs.ReactionDelay;
        _startFollowDist = configs.StartFollowDist;
        _stopFollowDist = configs.StopFollowDist;
        _followSpeedMax = configs.FollowSpeedMax;
        _followSpeedMin = configs.FollowSpeedMin;
        _maxSpeedDist = configs.MaxSpeedDist;
        _minSpeedDist = configs.MinSpeedDist;
        _followArcOffsetMax = configs.FollowArcOffsetMax;

        _followAccelerationCurveTracker.y = configs.FollowAccelerationCurve.keys[^1].time;
    }

    #region IUPDATABLE

    public void CustomUpdate() {

        // TODO: add offset to character pos
        float characterDist = (_characterTransform.position - transform.position).sqrMagnitude;
        CheckFollowPlayer();

        if (!_followingCharacter)
            return;

        if (_reactionTimer < _reactionDelay)
            _reactionTimer += Time.deltaTime;
        if (_reactionTimer < _reactionDelay)
            return;

        if (_followAccelerationCurveTracker.x < _followAccelerationCurveTracker.y)
            IncrementAccelerationCurveTracker();
        if (_followAccelerationCurveTracker.x < _followAccelerationCurveTracker.y)
            return;

        float distFactor = Mathf.Clamp01(Mathf.InverseLerp(_minSpeedDist, _maxSpeedDist, characterDist));
        float baseSpeed = Mathf.Lerp(_followSpeedMin, _followSpeedMax, distFactor);

        Vector2 companionCharacterVector = _characterTransform.position - transform.position;

        Vector2 arcDirection = Vector2.Perpendicular(companionCharacterVector.normalized);
        if (arcDirection.y > 0)
            arcDirection = new Vector2(-arcDirection.x, -arcDirection.y);

        Vector2 arcVector = arcDirection * _followArcVelocityCurve.Evaluate(distFactor) * _followArcOffsetMax;
        Vector2 directionVector = (companionCharacterVector + arcVector).normalized;

        _velocity = directionVector * baseSpeed * 
            _followAccelerationCurve.Evaluate(_followAccelerationCurveTracker.x);

        _rigidbody2D.velocity = _velocity;

        void CheckFollowPlayer() {
            
            if (_followingCharacter) {
                if (characterDist <= _stopFollowDist) {
                    _followingCharacter = false;
                    _velocity = Vector2.zero;
                    _rigidbody2D.velocity = _velocity;
                }
            } else {
                if (characterDist >= _startFollowDist) {
                    _followingCharacter = true;
                    _reactionTimer = 0;
                }
            }
        }

        void IncrementAccelerationCurveTracker() =>
            _followAccelerationCurveTracker.x += Time.deltaTime;
    }

    #endregion
}
