using UnityEngine;

namespace Gameplay
{
    public class CharacterSpriteController : GameplayComponent
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetCharacterOrientation(bool faceLeft) => spriteRenderer.flipX = faceLeft;
    }
}
