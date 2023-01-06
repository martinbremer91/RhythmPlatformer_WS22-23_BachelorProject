using System;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility_Scripts;

namespace Gameplay
{
    public class CharacterCollisionDetector : MonoBehaviour, IUpdatable, IInit<GameStateManager>
    {
        public UpdateType UpdateType => UpdateType.GamePlay;
        
        private CharacterStateController _characterStateController;
        private CharacterMovement _characterMovement;
        private CharacterInput _characterInput;
        [SerializeField] private BoxCollider2D _boxCollider;

        [SerializeField] private LayerMask _levelLayerMask;

        [HideInInspector] public bool SlideOnHorizontal;
        [HideInInspector] private int _slideOnHorizontalDirection;
        [HideInInspector] public bool SlideOnVertical;
        private int _slideOnVerticalDirection;
        private CollisionCheck _slideOnVerticalCollisionSide;
        private float _slideOnSpeed;

        [HideInInspector] public bool OnOneWayPlatform;
        private string _oneWayPlatformTag;
        
        public void Init(GameStateManager in_gameStateManager)
        {
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
            _characterInput = in_gameStateManager.CharacterInput;
            _slideOnSpeed = in_gameStateManager.MovementConfigs.SlideOnSpeed;

            _oneWayPlatformTag = Constants.OneWayPlatform;
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

            if (SlideOnHorizontal)
                transform.Translate(Vector2.right * _slideOnHorizontalDirection * _slideOnSpeed * Time.fixedDeltaTime);
            else if (SlideOnVertical)
                transform.Translate(Vector2.up * _slideOnVerticalDirection * _slideOnSpeed * Time.fixedDeltaTime);
        }

        private void DetectCollision(CollisionCheck in_collisionCheck, bool in_detectEnter)
        {
            Bounds bounds = _boxCollider.bounds;

            Vector2 detectDirection =
                in_collisionCheck is CollisionCheck.Ground ? Vector2.down :
                in_collisionCheck is CollisionCheck.LeftWall ? Vector2.left :
                in_collisionCheck is CollisionCheck.RightWall ? Vector2.right :
                Vector2.up;

            bool verticalDetection = in_collisionCheck is CollisionCheck.Ground or CollisionCheck.Ceiling;
            
            float radiusOnDetectionAxis =  verticalDetection ?
                bounds.size.y * .5f : bounds.size.x * .5f;
            float radiusOnComplementaryAxis = verticalDetection ?
                bounds.size.x * .51f + (SlideOnHorizontal ? .1f : 0): bounds.size.y * .5f;

            Vector2 pointOnDetectionAxis = 
                (Vector2)bounds.center + detectDirection * (radiusOnDetectionAxis + .05f);
            Vector2 offsetOnComplementaryAxis = verticalDetection ?
                Vector2.right * radiusOnComplementaryAxis : Vector2.up * radiusOnComplementaryAxis * .9f;

            Vector2 collisionCheckPointA = pointOnDetectionAxis + offsetOnComplementaryAxis;
            Vector2 collisionCheckPointB = pointOnDetectionAxis - offsetOnComplementaryAxis;

            // Note on Physics2D.OverlapPoint: will only work with composite colliders if their geometry type is
            // set to Polygons instead of Outlines

            Collider2D hitA = Physics2D.OverlapPoint(collisionCheckPointA, _levelLayerMask);
            Collider2D hitB = Physics2D.OverlapPoint(collisionCheckPointB, _levelLayerMask);

            bool collision = hitA != null && hitB != null;

            if (in_collisionCheck is CollisionCheck.Ground)
                SlideOnHorizontal = CheckHorizontalSlideOn();
            else if (!verticalDetection)
                SlideOnVertical = CheckVerticalSlideOn();
            else if (collision && hitA.gameObject.CompareTag(_oneWayPlatformTag))
                collision = OnOneWayPlatform;

            if (_characterStateController.CurrentCharacterState is CharacterState.Fall) {
                bool collisionOutsideOfCollisionChecks = 
                    !collision && _boxCollider.IsTouchingLayers(_levelLayerMask);

                if (collisionOutsideOfCollisionChecks) {
                    bool collisionRight = Physics2D.OverlapPoint(collisionCheckPointA + Vector2.right * .1f, _levelLayerMask);
                    bool collisionLeft = false;

                    if (collisionRight)
                        transform.Translate(Vector2.left * .01f);
                    else 
                        collisionLeft = Physics2D.OverlapPoint(collisionCheckPointB + Vector2.left * .1f, _levelLayerMask);

                    if (collisionLeft)
                        transform.Translate(Vector2.right * .01f);
                }
            }

#if UNITY_EDITOR
            // COLLISION DEBUGGING
            Color color = collision ? Color.green : verticalDetection ? Color.yellow : Color.cyan;
            Debug.DrawLine(collisionCheckPointA, collisionCheckPointB, color);
            Debug.DrawLine(bounds.center, (Vector2)bounds.center + detectDirection, collision ? Color.green : Color.red);

            color = hitA == null ? Color.red : Color.green;
            Debug.DrawLine(collisionCheckPointA + new Vector2(-.05f, .05f), collisionCheckPointA + new Vector2(.05f, -.05f), color);
            Debug.DrawLine(collisionCheckPointA + new Vector2(-.05f, -.05f), collisionCheckPointA + new Vector2(.05f, .05f), color);

            color = hitB == null ? Color.red : Color.green;
            Debug.DrawLine(collisionCheckPointB + new Vector2(-.05f, .05f), collisionCheckPointB + new Vector2(.05f, -.05f), color);
            Debug.DrawLine(collisionCheckPointB + new Vector2(-.05f, -.05f), collisionCheckPointB + new Vector2(.05f, .05f), color);
#endif

            if (!collision == in_detectEnter)
                return;

            if (CheckValidStateForCollisionInteraction(in_collisionCheck, in_detectEnter))
                _characterStateController.HandleCollisionStateChange(in_collisionCheck, in_detectEnter);

            bool CheckHorizontalSlideOn()
            {
                bool slideOnToLeft = hitA == null && hitB != null;
                bool slideOnToRight = hitA != null && hitB == null;

                if (collision || !slideOnToLeft && !slideOnToRight)
                    return false;

                _slideOnHorizontalDirection = slideOnToLeft ? -1 : 1;
                collision = true;
                bool running = 
                    _slideOnHorizontalDirection * Mathf.RoundToInt(_characterInput.InputState.DirectionalInput.x) != 0;

                bool platCheck = hitA != null && hitA.CompareTag(Constants.OneWayPlatform) ||
                    hitB != null && hitB.CompareTag(Constants.OneWayPlatform);

                return !running && platCheck == OnOneWayPlatform;
            }

            bool CheckVerticalSlideOn() {
                // if slideOn is active on opposite side, return true so as not to disrupt it
                if (_slideOnVerticalCollisionSide is not CollisionCheck.None &&
                    _slideOnVerticalCollisionSide != in_collisionCheck)
                    return true;
                
                bool slideOnDown = hitA == null && hitB != null;
                bool slideOnUp = hitA != null && hitB == null;

                if (collision || !slideOnDown && !slideOnUp) {
                    _slideOnVerticalCollisionSide = CollisionCheck.None;
                    return false;
                }

                bool hitIsOneWayPlat = slideOnDown && hitB.CompareTag(_oneWayPlatformTag) || 
                    slideOnUp && hitA.CompareTag(_oneWayPlatformTag);

                if (hitIsOneWayPlat) {
                    collision = false;
                    _slideOnVerticalCollisionSide = CollisionCheck.None;
                    return false;
                }

                collision = true;

                _slideOnVerticalDirection = slideOnDown ? -1 : 1;
                bool clinging = 
                    _characterInput.InputState.WallClingTrigger is InputActionPhase.Performed;

                _slideOnVerticalCollisionSide = 
                    !clinging ? CollisionCheck.None : in_collisionCheck;

                return clinging;
            }
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
