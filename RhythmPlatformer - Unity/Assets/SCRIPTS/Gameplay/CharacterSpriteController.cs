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
        [SerializeField] private ColorPalette _characterColorPalette;
        private PulseMaterialOverrides _pulseMaterialOverrides;

        #endregion

        private LabeledColor _defaultColors;
        private LabeledColor _noDashColors;

        public Animator Animator
        {
            get => _playerAnimator;
            set => _playerAnimator = value;
        }

        public void Init(GameStateManager in_gameStateManager) {
            _characterStateController = in_gameStateManager.CharacterStateController;
            _pulseMaterialOverrides = new PulseMaterialOverrides(_spriteRenderer);

            _defaultColors = _characterColorPalette.GetColorByLabel("Default");
            _noDashColors = _characterColorPalette.GetColorByLabel("NoDash");
        }

        float timer = 1;
        bool done; 
        private void Update() {
            timer -= Time.deltaTime;

            if (!done && timer <= 0) {
                done = true;
                SetSilhouetteDistance(.3f);
            }
        }

        public void SetCharacterOrientation(bool in_faceLeft) {
            _spriteRenderer.flipX = in_faceLeft;
            _pulseMaterialOverrides.SetFlipX(in_faceLeft);
        }

        public void SetSilhouetteDistance(float in_distancePercentage) {
            _pulseMaterialOverrides.SetSilhouetteDistance(in_distancePercentage * 
                _pulseMaterialOverrides.OutlineThicknessMax);
        }

        public void SetDashWindupTrigger()
        {
            ResetAnimationTriggers();
            _playerAnimator.SetTrigger(Constants.DashWindupClipName);
        }

        private void ResetAnimationTriggers()
        {
            _playerAnimator.ResetTrigger(Constants.IdleClipName);
            _playerAnimator.ResetTrigger(Constants.RunClipName);
            _playerAnimator.ResetTrigger(Constants.LandClipName);
            _playerAnimator.ResetTrigger(Constants.RiseClipName);
            _playerAnimator.ResetTrigger(Constants.FallClipName);
            _playerAnimator.ResetTrigger(Constants.DashWindupClipName);
            _playerAnimator.ResetTrigger(Constants.DashClipName);
            _playerAnimator.ResetTrigger(Constants.WalledClipName);
        }

        public void HandleStateAnimation()
        {
            ResetAnimationTriggers();

            switch (_characterStateController.CurrentCharacterState)
            {
                case CharacterState.Idle:
                    _playerAnimator.SetTrigger(Constants.IdleClipName);
                    break;
                case CharacterState.Crouch:
                    _playerAnimator.SetTrigger(Constants.LandClipName);
                    break;
                case CharacterState.Run:
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
        
        public void UpdateCanDashColor(bool in_canDash) {
            LabeledColor colors = in_canDash ? _defaultColors : _noDashColors;
            
            _pulseMaterialOverrides.ChangeBaseColor(colors.Color);
            _pulseMaterialOverrides.ChangeSecondaryColor(colors.HDRColor);
        }
    }
}
