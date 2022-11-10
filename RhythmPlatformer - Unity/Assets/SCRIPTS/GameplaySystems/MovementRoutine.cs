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

        // TODO: change this type to a parent of OneWayPlatform
        [SerializeField] private OneWayPlatform _movingObject;
        [SerializeField] private List<Waypoint> _waypoints;
        public List<Waypoint> Waypoints => _waypoints ??= new List<Waypoint>();

        #endregion

        private Vector3 _position => _movingObject.transform.position;
        
        private Waypoint _currentWaypoint;
        private Vector3 _currentWaypointDirection;
        private int _nextWaypointIndex;
        private bool _hasCurrentWaypoint;

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

            _movingObject.Init(in_gameStateManager);
            
            if (Waypoints == null || !Waypoints.Any())
                Debug.LogError(name + "'s MovementRoutine does not have waypoints", gameObject);
        }

        #endregion

        private void FollowWaypoints()
        {
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
                _currentWaypointDirection = (_currentWaypoint.Coords - _position).normalized;
                _nextWaypointIndex++;

                if (_nextWaypointIndex >= Waypoints.Count)
                    _nextWaypointIndex = 0;
            }

            bool CheckIfCurrentWaypointWasReached()
            {
                bool waypointReached = (_currentWaypoint.Coords - _position).sqrMagnitude <= .1f;
                return waypointReached;
            }

            void MoveTowardsCurrentWaypoint() =>
                _movingObject.transform.Translate(_currentWaypointDirection * (5 * Time.deltaTime));

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


