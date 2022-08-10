using UnityEngine;

namespace Gameplay
{
    public class CharacterSpriteController : GameplayComponent
    {
        [SerializeField] private CharacterStateController _characterStateController;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public Animator PlayerAnimator;

        public void SetCharacterOrientation(bool in_faceLeft) => _spriteRenderer.flipX = in_faceLeft;

        protected override void OnEnable()
        {
            base.OnEnable();
            _characterStateController.JumpSquatStarted += SetJumpSquatTrigger;
            _characterStateController.DashWindupStarted += SetDashWindupTrigger;
        }

        private void SetJumpSquatTrigger() => PlayerAnimator.SetTrigger(Constants.JumpSquatClipName);
        private void SetDashWindupTrigger() => PlayerAnimator.SetTrigger(Constants.DashWindupClipName);

        protected override void OnDisable()
        {
            base.OnDisable();
            _characterStateController.JumpSquatStarted -= SetJumpSquatTrigger;
            _characterStateController.DashWindupStarted -= SetDashWindupTrigger;
        }
    }
}
