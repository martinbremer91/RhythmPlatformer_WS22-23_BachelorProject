using UnityEngine;

namespace Gameplay
{
    public class CharacterSpriteController : GameplayComponent
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool characterFacingLeft => spriteRenderer.flipX;

        public void SetCharacterOrientation(bool faceLeft) => spriteRenderer.flipX = faceLeft;
    }
}
