using System;
using Systems;
using UnityEngine;

namespace Gameplay
{
    public class CharacterCollisionChecks : GameplayComponent
    {
        [SerializeField] private CharacterStateController _characterStateController;
        [SerializeField] private CollisionCheck _collisionCheck;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.LevelTag))
                return;
            
            if (CheckValidStateForCollisionInteraction(true))
                _characterStateController.HandleCollisionStateChange(_collisionCheck, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.LevelTag))
                return;
            
            if (CheckValidStateForCollisionInteraction(false))
                _characterStateController.HandleCollisionStateChange(_collisionCheck, false);
        }

        private bool CheckValidStateForCollisionInteraction(bool in_enter)
        {
            switch (_collisionCheck)
            {
                case CollisionCheck.Ground: 
                    return in_enter != _characterStateController.Grounded;
                case CollisionCheck.Ceiling:
                    return in_enter != (_characterStateController.CurrentCharacterState == CharacterState.Fall);
                case CollisionCheck.LWall:
                    _characterStateController.NearWall_L = in_enter;
                    if (in_enter && !_characterStateController.Airborne)
                        return false;
                    return in_enter != _characterStateController.Walled;
                case CollisionCheck.RWall:
                    _characterStateController.NearWall_R = in_enter;
                    if (in_enter && !_characterStateController.Airborne) 
                        return false;
                    return in_enter != _characterStateController.Walled;
            }
            
            throw new Exception("Could not find valid collision check type");
        }
    }
}
