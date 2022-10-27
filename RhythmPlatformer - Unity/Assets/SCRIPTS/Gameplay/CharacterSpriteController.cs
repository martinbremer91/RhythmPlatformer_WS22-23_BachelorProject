using Interfaces_and_Enums;
using GlobalSystems;
using UnityEngine;
using Utility_Scripts;

namespace Gameplay
{
    public class CharacterSpriteController : MonoBehaviour, IInit<GameStateManager>
    {
        #region REFERENCES

        private CharacterStateController _characterStateController;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public Animator PlayerAnimator;

        #endregion

        public void SetCharacterOrientation(bool in_faceLeft) => _spriteRenderer.flipX = in_faceLeft;

        public void Init(GameStateManager in_gameStateManager) =>
            _characterStateController = in_gameStateManager.CharacterStateController;

        public void SetDashWindupTrigger() => PlayerAnimator.SetTrigger(Constants.DashWindupClipName);

        public void HandleStateAnimation()
        {
            switch (_characterStateController.CurrentCharacterState)
            {
                case CharacterState.Idle:
                    PlayerAnimator.ResetTrigger(Constants.LandClipName);
                    PlayerAnimator.SetTrigger(Constants.IdleClipName);
                    break;
                case CharacterState.Crouch:
                    PlayerAnimator.SetTrigger(Constants.LandClipName);
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
    }
}
