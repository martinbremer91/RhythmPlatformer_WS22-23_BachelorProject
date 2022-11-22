using System;
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
        private Transform _characterTransform;

        [SerializeField] private float _moveSpeed = 5;

        public List<Waypoint> Waypoints;

        #endregion

        private Waypoint _currentWaypoint;
        private Vector3 _currentWaypointDirection;
        private Vector3 _previousWaypointCoords;
        private int _nextWaypointIndex;
        private bool _hasCurrentWaypoint;

        [HideInInspector] public bool MovePlayerAsWell;

        private bool _pausingBetweenWaypoints;
        private float _pausingTimer;

        private void OnDisable() => _updateManager.MovementRoutines.Remove(this);

        #region IUpdatable
        
        public UpdateType UpdateType => UpdateType.GamePlay;
        public static UpdateType s_UpdateType => UpdateType.GamePlay;
        
        public void CustomUpdate() => FollowWaypoints();

        #endregion

        #region IInit

        public void Init(GameStateManager in_gameStateManager)
        {
            _updateManager = in_gameStateManager.UpdateManager;
            _updateManager.MovementRoutines.Add(this);
            _characterTransform = in_gameStateManager.CharacterStateController.transform;
            _previousWaypointCoords = transform.position;
        }

        #endregion

        private void Start()
        {
            if (Waypoints == null || !Waypoints.Any())
            {
                Debug.LogError("Movement Routine has no waypoints. Disabling Movement Routine.", gameObject);
                gameObject.SetActive(false);
            }
        }

        private void FollowWaypoints()
        {
            Vector3 position = transform.position;

            if (!_hasCurrentWaypoint)
                GetNextWaypoint();

            if (!_pausingBetweenWaypoints && !CheckIfCurrentWaypointWasReached())
                MoveTowardsCurrentWaypoint();
            else
                HandlePausingBetweenWaypoints();

            void GetNextWaypoint()
            {
                _hasCurrentWaypoint = true;

                _previousWaypointCoords = _currentWaypoint.Coords;
                _currentWaypoint = Waypoints[_nextWaypointIndex];
                _currentWaypointDirection = ((Vector3)_currentWaypoint.Coords - position).normalized;
                _nextWaypointIndex++;

                if (_nextWaypointIndex >= Waypoints.Count)
                    _nextWaypointIndex = 0;
            }

            bool CheckIfCurrentWaypointWasReached()
            {
                bool waypointReached = _previousWaypointCoords.InverseLerp(_currentWaypoint.Coords, position) >= 1;
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
                _pausingBetweenWaypoints = true;
                _pausingTimer += Time.deltaTime;

                if (_pausingTimer >= _currentWaypoint.Pause)
                {
                    _pausingTimer = 0;
                    _pausingBetweenWaypoints = false;
                    _hasCurrentWaypoint = false;
                }
            }
        }
    }
}


