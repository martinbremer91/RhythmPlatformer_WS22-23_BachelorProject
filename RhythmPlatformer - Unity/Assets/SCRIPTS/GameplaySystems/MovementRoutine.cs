using System;
using Structs;
using System.Collections.Generic;
using System.Linq;
using Interfaces_and_Enums;
using UnityEngine;
using GlobalSystems;

namespace GameplaySystems
{
    public class MovementRoutine : MonoBehaviour, IInit<GameStateManager>,IUpdatable
    {
        #region REFERENCES / EXPOSED FIELDS

        [HideInInspector] public UpdateManager _updateManager;
        private Transform _characterTransform;

        public List<Waypoint> Waypoints;

        #endregion

        private Waypoint _currentWaypoint;
        private Vector3 _currentWaypointDirection;
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
                
                _currentWaypoint = Waypoints[_nextWaypointIndex];
                _currentWaypointDirection = ((Vector3)_currentWaypoint.Coords - position).normalized;
                _nextWaypointIndex++;

                if (_nextWaypointIndex >= Waypoints.Count)
                    _nextWaypointIndex = 0;
            }

            bool CheckIfCurrentWaypointWasReached()
            {
                bool waypointReached = ((Vector3)_currentWaypoint.Coords - position).sqrMagnitude <= .1f;
                return waypointReached;
            }

            void MoveTowardsCurrentWaypoint()
            {
                Vector3 translation = _currentWaypointDirection * (5 * Time.deltaTime);
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


