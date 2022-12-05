using Structs;
using System.Collections.Generic;
using System.Linq;
using Interfaces_and_Enums;
using UnityEngine;
using GlobalSystems;

namespace GameplaySystems
{
    public class MovementRoutine : MonoBehaviour, IInit<GameStateManager>, IUpdatable
    {
        #region REFERENCES / EXPOSED FIELDS

        [HideInInspector] public UpdateManager _updateManager;
        private BeatManager _beatManager;
        private Transform _characterTransform;

        private float _moveSpeed;
        private float _beatLength;

        public List<Waypoint> Waypoints;

        #endregion

        #region VARIABLES

        private Waypoint _currentWaypoint;
        private Vector3 _currentWaypointDirection;
        private Waypoint _previousWaypoint;
        private int _nextWaypointIndex;
        private bool _hasCurrentWaypoint;

        private int _nextArrivalBeat;
        private int _nextDepartureBeat;

        [HideInInspector] public bool MovePlayerAsWell;

        private bool _waitingForDepartureBeat;

        #endregion

        #region IUpdatable
        
        public UpdateType UpdateType => UpdateType.GamePlay;
        public static UpdateType s_UpdateType => UpdateType.GamePlay;
        
        public void CustomUpdate() => FollowWaypoints();

        #endregion

        #region INITIALIZATION

        private void OnDisable() => _updateManager.MovementRoutines.Remove(this);

        public void Init(GameStateManager in_gameStateManager)
        {
            _updateManager = in_gameStateManager.UpdateManager;
            _beatManager = in_gameStateManager.BeatManager;
            _beatLength = (float)_beatManager.BeatLength;
            _updateManager.MovementRoutines.Add(this);
            _characterTransform = in_gameStateManager.CharacterStateController.transform;
            _currentWaypoint = new Waypoint(transform.position);

            _nextDepartureBeat = GetNextMovementBeat(Waypoints[Waypoints.Count - 1].DepartureBeats);
        }

        private void Start()
        {
            if (Waypoints == null || !Waypoints.Any())
            {
                Debug.LogError("Movement Routine has no waypoints. Disabling Movement Routine.", gameObject);
                gameObject.SetActive(false);
            }
        }

        #endregion

        private void FollowWaypoints()
        {
            Vector3 position = transform.position;

            if (!_hasCurrentWaypoint)
                GetNextWaypoint();

            if (!_waitingForDepartureBeat && !CheckIfCurrentWaypointWasReached())
                MoveTowardsCurrentWaypoint();
            else
                HandlePausingBetweenWaypoints();

            void GetNextWaypoint()
            {
                _hasCurrentWaypoint = true;

                _previousWaypoint = _currentWaypoint;
                _currentWaypoint = Waypoints[_nextWaypointIndex];
                _currentWaypointDirection = ((Vector3)_currentWaypoint.Coords - position).normalized;
                _nextWaypointIndex++;

                if (_nextWaypointIndex >= Waypoints.Count)
                    _nextWaypointIndex = 0;

                _nextArrivalBeat = GetNextMovementBeat(_currentWaypoint.ArrivalBeats);
                _moveSpeed = GetMoveSpeed(_previousWaypoint.Coords, _currentWaypoint.Coords);
            }

            bool CheckIfCurrentWaypointWasReached()
            {
                bool waypointReached = _previousWaypoint.Coords.InverseLerp(_currentWaypoint.Coords, position) >= 1;
                
                if (waypointReached)
                    _nextDepartureBeat = GetNextMovementBeat(_currentWaypoint.DepartureBeats);

                return waypointReached;
            }

            void MoveTowardsCurrentWaypoint()
            {
                Vector3 translation = _currentWaypointDirection * (_moveSpeed * Time.deltaTime);
                transform.Translate(translation);
                
                if (MovePlayerAsWell)
                    _characterTransform.Translate(translation);
            }

            void HandlePausingBetweenWaypoints()
            {
                _waitingForDepartureBeat = true;

                if (_beatManager.BeatTracker == _nextDepartureBeat)
                {
                    _waitingForDepartureBeat = false;
                    _hasCurrentWaypoint = false;
                }
            }

            float GetMoveSpeed(Vector2 in_origin, Vector2 in_destination) {
                int beatsInMovement = _nextArrivalBeat > _nextDepartureBeat ? _nextDepartureBeat - _nextArrivalBeat : 
                    _beatManager.Meter - _nextDepartureBeat + _nextArrivalBeat;

                return Mathf.Abs(Vector2.Distance(in_origin, in_destination) / beatsInMovement * _beatLength);
            }
        }

        private int GetNextMovementBeat(int[] in_movementBeats) {
            bool nextBeatInCurrentBar = in_movementBeats.Any(b => b >= _beatManager.BeatTracker);
            return nextBeatInCurrentBar ?
                in_movementBeats.Where(b => b >= _beatManager.BeatTracker).Min() : in_movementBeats.Min();
        }
    }
}
