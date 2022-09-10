using UnityEngine;
using Utility_Scripts;

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
            _characterStateController.DashWindupStarted += SetDashWindupTrigger;
        }
        
        private void SetDashWindupTrigger() => PlayerAnimator.SetTrigger(Constants.DashWindupClipName);

        public void HandleStateAnimation()
        {
            switch (_characterStateController.CurrentCharacterState)
            {
                case CharacterState.Idle:
                    PlayerAnimator.SetTrigger(Constants.IdleClipName);
                    break;
                case CharacterState.Run:
                    PlayerAnimator.SetTrigger(Constants.RunClipName);
                    break;
                case CharacterState.Land:
                    PlayerAnimator.SetTrigger(Constants.LandClipName);
                    break;
                case CharacterState.Rise:
                    PlayerAnimator.SetTrigger(Constants.RiseClipName);
                    break;
                case CharacterState.Fall:
                    PlayerAnimator.SetTrigger(Constants.FallClipName);
                    break;
                case CharacterState.Dash:
                    PlayerAnimator.SetTrigger(Constants.DashClipName);
                    break;
                case CharacterState.WallCling:
                case CharacterState.WallSlide:
                    PlayerAnimator.SetTrigger(Constants.WalledClipName);
                    break;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _characterStateController.DashWindupStarted -= SetDashWindupTrigger;
        }
    }
}
