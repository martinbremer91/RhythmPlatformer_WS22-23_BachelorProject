using Interfaces_and_Enums;
using GlobalSystems;
using UnityEngine;
using Utility_Scripts;

namespace Gameplay
{
    public class CharacterSpriteController : MonoBehaviour, IInit<GameStateManager>, IAnimatorPausable
    {
        #region REFERENCES

        private CharacterStateController _characterStateController;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _playerAnimator;

        #endregion

        public Animator Animator
        {
            get => _playerAnimator;
            set => _playerAnimator = value;
        }

        public void SetCharacterOrientation(bool in_faceLeft) => _spriteRenderer.flipX = in_faceLeft;

        public void Init(GameStateManager in_gameStateManager) =>
            _characterStateController = in_gameStateManager.CharacterStateController;

        public void SetDashWindupTrigger() => _playerAnimator.SetTrigger(Constants.DashWindupClipName);

        public void HandleStateAnimation()
        {
            switch (_characterStateController.CurrentCharacterState)
            {
                case CharacterState.Idle:
                    _playerAnimator.ResetTrigger(Constants.LandClipName);
                    _playerAnimator.SetTrigger(Constants.IdleClipName);
                    break;
                case CharacterState.Crouch:
                    _playerAnimator.ResetTrigger(Constants.LandClipName);
                    _playerAnimator.SetTrigger(Constants.LandClipName);
                    break;
                case CharacterState.Run:
                    _playerAnimator.ResetTrigger(Constants.LandClipName);
                    _playerAnimator.SetTrigger(Constants.RunClipName);
                    break;
                case CharacterState.Land:
                    _playerAnimator.SetTrigger(Constants.LandClipName);
                    break;
                case CharacterState.Rise:
                    _playerAnimator.SetTrigger(Constants.RiseClipName);
                    break;
                case CharacterState.Fall:
                    _playerAnimator.SetTrigger(Constants.FallClipName);
                    break;
                case CharacterState.Dash:
                    _playerAnimator.SetTrigger(Constants.DashClipName);
                    break;
                case CharacterState.WallCling:
                case CharacterState.WallSlide:
                    _playerAnimator.SetTrigger(Constants.WalledClipName);
                    break;
            }
        }

        public void OnTogglePause(bool in_paused) => (this as IAnimatorPausable).ToggleAnimatorPause(in_paused);
    }
}
