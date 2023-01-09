using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public class CompanionMovement : MonoBehaviour, IUpdatable, IInit<GameStateManager>, IPhysicsPausable
{
    public UpdateType UpdateType => UpdateType.GamePlay;

    public Rigidbody2D PausableRigidbody => _rigidbody2D;
    
    private Vector2 _velocity;
    public Vector2 Velocity {
        get => _velocity;
        set {
            _velocity = value;
            CurrentCompanionState = value.magnitude > 3 ?
                CompanionState.Fly : CompanionState.Idle;
        }
    }

    private GameStateManager _gameStateManager;
    private CharacterStateController _characterStateController;
    private Transform _characterTransform;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private CompanionSpriteController _companionSpriteController;

    private CompanionState _currentCompanionState;
    public CompanionState CurrentCompanionState {
        get => _currentCompanionState;
        private set {
            _currentCompanionState = value;
            _companionSpriteController.HandleStateAnimation();
        }
    }

    private bool m_facingLeft;
    private bool _facingLeft {
        get => m_facingLeft;
        set {
            if (m_facingLeft == value)
                return;

            m_facingLeft = value;
            _companionSpriteController.HandleOrientationChange(value);
        }
    }

    [HideInInspector] public Vector2 CharacterOffset;

    private AnimationCurve _followAccelerationCurve;
    private AnimationCurve _followArcVelocityCurve;
    private Vector2 _followAccelerationCurveTracker;

    private float _followSpeedMax;
    private float _followSpeedMin;

    private float _followArcOffsetMax;

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
        if (_gameStateManager != null && !_gameStateManager.PhysicsPausables.Contains(this))
            _gameStateManager.PhysicsPausables.Add(this);
    }

    public void DeregisterPhysicsPausable() =>
         _gameStateManager.PhysicsPausables.Remove(this);

    public void Init(GameStateManager in_gameStateManager) {
        _gameStateManager = in_gameStateManager;
        RegisterPhysicsPausable();
        CompanionConfigs configs = in_gameStateManager.CompanionConfigs;

        _characterStateController = in_gameStateManager.CharacterStateController;
        _characterTransform = _characterStateController.transform;

        _followAccelerationCurve = configs.FollowAccelerationCurve;
        _followArcVelocityCurve = configs.FollowArcVelocityCurve;
        CharacterOffset = configs.CharacterOffset;
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

        Vector2 position = transform.position;
        Vector2 targetPos = (Vector2)_characterTransform.position + new Vector2(_characterStateController.FacingLeft ? 
            -CharacterOffset.x : CharacterOffset.x, CharacterOffset.y);

        float characterDist = (position - targetPos).sqrMagnitude;
        CheckFollowPlayer();

        UpdateSpriteOrientation();

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

        Vector2 companionCharacterVector = targetPos - position;

        Vector2 arcDirection = Vector2.Perpendicular(companionCharacterVector.normalized);
        if (arcDirection.y < 0)
            arcDirection = new Vector2(-arcDirection.x, -arcDirection.y);

        Vector2 arcVector = arcDirection * _followArcVelocityCurve.Evaluate(distFactor) * _followArcOffsetMax;
        Vector2 directionVector = (companionCharacterVector + arcVector).normalized;

        Velocity = directionVector * baseSpeed * 
            _followAccelerationCurve.Evaluate(_followAccelerationCurveTracker.x);

        _rigidbody2D.velocity = Velocity;

        Quaternion moveRotation = CurrentCompanionState is CompanionState.Fly ? 
            Quaternion.LookRotation(Vector3.forward,
            _facingLeft ? -Vector2.Perpendicular(Velocity.normalized) : Vector2.Perpendicular(Velocity.normalized)) : 
            Quaternion.identity;
        transform.rotation = moveRotation;

        void CheckFollowPlayer() {
            
            if (_followingCharacter) {
                if (characterDist <= _stopFollowDist) {
                    _followingCharacter = false;
                    Velocity = Vector2.zero;
                    _rigidbody2D.velocity = Velocity;
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

        void UpdateSpriteOrientation() {
            if (CurrentCompanionState is CompanionState.Idle) {
                _facingLeft = _characterTransform.position.x < transform.position.x;
            } else {
                _facingLeft = Velocity.x < 0;
            }
        }
    }

    #endregion
}
