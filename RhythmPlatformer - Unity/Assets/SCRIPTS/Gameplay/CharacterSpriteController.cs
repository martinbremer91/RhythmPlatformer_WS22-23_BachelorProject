using UnityEngine;

namespace Gameplay
{
    public class CharacterSpriteController : GameplayComponent
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool canTurn => CharacterState.CanTurn.HasFlag(CharacterStateDetector.CurrentCharacterState);

        public bool characterFacingLeft => spriteRenderer.flipX;

        public void SetCharacterOrientation(bool faceLeft)
        {
            if (canTurn)
                spriteRenderer.flipX = faceLeft;
        } 
    }
}
