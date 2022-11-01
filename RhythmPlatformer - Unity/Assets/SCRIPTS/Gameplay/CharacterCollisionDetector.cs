using System;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace Gameplay
{
    public class CharacterCollisionDetector : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        private CharacterStateController _characterStateController;
        private CharacterMovement _characterMovement;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _levelLayerMask;
        [SerializeField] private float _detectionOffset;

        public void Init(GameStateManager in_gameStateManager)
        {
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
        }
        
        public void CustomUpdate()
        {
            DetectCollision(CollisionCheck.Ground, !_characterStateController.Grounded);

            if (!_characterStateController.Grounded)
            {
                DetectCollision(CollisionCheck.Ceiling, !_characterStateController.CeilingHit);
                DetectCollision(CollisionCheck.LeftWall, !_characterStateController.NearWallLeft);
                DetectCollision(CollisionCheck.RightWall, !_characterStateController.NearWallRight);
            }
        }

        private void DetectCollision(CollisionCheck in_collisionCheck, bool in_detectEnter)
        {
            Bounds bounds = _boxCollider.bounds;

            Vector2 detectDirection =
                in_collisionCheck == CollisionCheck.Ground ? Vector2.down :
                in_collisionCheck == CollisionCheck.LeftWall ? Vector2.left :
                in_collisionCheck == CollisionCheck.RightWall ? Vector2.right :
                Vector2.up;

            RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size * 1.01f, 0f, 
                detectDirection, _detectionOffset, _levelLayerMask);

            bool collision = hit.collider != null;

            // COLLISION DEBUGGING
            Color color = collision ? Color.green : Color.red;
            Debug.DrawRay(bounds.center, detectDirection, color);

            if (!collision == in_detectEnter)
                return;
            
            // COLLISION DEBUGGING
            if (collision)
                Debug.Log(in_collisionCheck + ": " + hit.collider.name + " -> enter", hit.collider.gameObject);
            else
                Debug.Log(in_collisionCheck + ": <none> -> exit");

            if (CheckValidStateForCollisionInteraction(in_collisionCheck, in_detectEnter))
                _characterStateController.HandleCollisionStateChange(in_collisionCheck, in_detectEnter);
        }

        private bool CheckValidStateForCollisionInteraction(CollisionCheck in_collisionCheck, bool in_enter)
        {
            switch (in_collisionCheck)
            {
                case CollisionCheck.Ground:
                    if (in_enter && _characterMovement.CharacterVelocity.y > 0)
                        return false;
                    return in_enter != _characterStateController.Grounded;
                case CollisionCheck.Ceiling:
                    return in_enter != _characterStateController.CurrentCharacterState is CharacterState.Fall;
                case CollisionCheck.LeftWall:
                    _characterStateController.NearWallLeft = in_enter;
                    if (in_enter && !_characterStateController.Airborne)
                        return false;
                    return in_enter != _characterStateController.Walled;
                case CollisionCheck.RightWall:
                    _characterStateController.NearWallRight = in_enter;
                    if (in_enter && !_characterStateController.Airborne) 
                        return false;
                    return in_enter != _characterStateController.Walled;
            }
            
            throw new Exception("Could not find valid collision check type");
        }
    }
}
