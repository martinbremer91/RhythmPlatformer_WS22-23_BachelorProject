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
        private CharacterState correspondingState;

        private void Start()
        {
            correspondingState = GetCorrespondingState();
            characterStateController = ReferenceManager.Instance.CharacterStateController;
        }

        private CharacterState GetCorrespondingState()
        {
            switch (collisionCheck)
            {
                case CollisionCheck.Ground:
                    return CharacterState.Grounded;
                case CollisionCheck.Ceiling:
                    return CharacterState.Airborne;
                case CollisionCheck.LWall:
                    return CharacterState.Walled;
                case CollisionCheck.RWall:
                    return CharacterState.Walled;
            }

            throw new Exception("Could not find valid corresponding character state");
        }

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
            // Check if corresponding state is already true / false
            if (correspondingState.HasFlag(CharacterStateController.CurrentCharacterState) == enter)
                return false;

            switch (collisionCheck)
            {
                case CollisionCheck.Ground:
                    return true;
                case CollisionCheck.Ceiling:
                    return true;
                case CollisionCheck.LWall:
                    if (CharacterStateController.CurrentCharacterState.HasFlag(CharacterState.Grounded))
                        return false;
                    return enter == (CharacterMovement.CharacterVelocity.x <= 0 &&
                           CharacterInput.InputState.DirectionalInput.x < 0);
                case CollisionCheck.RWall:
                    if (CharacterStateController.CurrentCharacterState.HasFlag(CharacterState.Grounded))
                        return false;
                    return enter == (CharacterMovement.CharacterVelocity.x >= 0 &&
                              CharacterInput.InputState.DirectionalInput.x > 0);
            }
            
            throw new Exception("Could not find valid collision check type");
        }
    }
}
