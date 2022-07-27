using System;
using Systems;
using UnityEngine;

namespace Gameplay
{
    public class CharacterCollisionChecks : GameplayComponent
    {
        public enum CollisionCheck
        {
            Ground,
            Ceiling,
            RWall,
            LWall
        }

        private static CharacterStateController characterStateController;
        
        [SerializeField] private CollisionCheck collisionCheck;

        private void Start() => characterStateController = ReferenceManager.Instance.CharacterStateController;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.LevelTag))
                return;
            
            if (CheckValidStateForCollisionInteraction(true))
                characterStateController.HandleCollisionStateChange(collisionCheck, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.LevelTag))
                return;
            
            if (CheckValidStateForCollisionInteraction(false))
                characterStateController.HandleCollisionStateChange(collisionCheck, false);
        }

        private bool CheckValidStateForCollisionInteraction(bool enter)
        {
            switch (collisionCheck)
            {
                case CollisionCheck.Ground: 
                    return enter != CharacterStateController.Grounded;
                case CollisionCheck.Ceiling:
                    return enter != (CharacterStateController.CurrentCharacterState == CharacterState.Fall);
                case CollisionCheck.LWall:
                    CharacterStateController.NearWall_L = enter;
                    if (enter && !CharacterStateController.Airborne)
                        return false;
                    return enter != CharacterStateController.Walled;
                case CollisionCheck.RWall:
                    CharacterStateController.NearWall_R = enter;
                    if (enter && !CharacterStateController.Airborne) 
                        return false;
                    return enter != CharacterStateController.Walled;
            }
            
            throw new Exception("Could not find valid collision check type");
        }
    }
}
