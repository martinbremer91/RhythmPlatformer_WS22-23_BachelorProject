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
        private CharacterMovement _characterMovement;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _dashPreviewArrow;
        [SerializeField] private Animator _playerAnimator;
        private SilhouetteMaterialOverrides _pulseAndSilhouetteMatOverrides;

        #endregion

        #region VARIABLES
        
        private VisualsData _characterVisualsData;

        private float _silhouetteMatMaxDistance;
        private Vector2 _silhouetteMatProximityAlphaRange;

        private bool _nextBeatStrong;

        private LabeledColor _silhouetteWeakBeatColors;
        private LabeledColor _silhouetteStrongBeatColors;

        public Animator Animator
        {
            get => _playerAnimator;
            set => _playerAnimator = value;
        }

        #endregion

        public void Init(GameStateManager in_gameStateManager) {
            _characterStateController = in_gameStateManager.CharacterStateController;
            _characterMovement = in_gameStateManager.CharacterMovement;
            _pulseAndSilhouetteMatOverrides = new SilhouetteMaterialOverrides(_spriteRenderer);

            _characterVisualsData = in_gameStateManager.VisualsData;

            _silhouetteMatMaxDistance = _characterVisualsData.SilhouetteMatMaxDistance;
            _silhouetteMatProximityAlphaRange = _characterVisualsData.SilhouetteMatProximityAlphaRange;

            LabeledColor[] characterColors = _characterVisualsData.CharacterColors.ToArray();

            _silhouetteWeakBeatColors = 
                _characterVisualsData.GetColorByLabel(characterColors, "SilhouetteWeakBeat");
            _silhouetteStrongBeatColors = 
                _characterVisualsData.GetColorByLabel(characterColors, "SilhouetteStrongBeat");
        }

        public void SetCharacterOrientation(bool in_faceLeft) {
            _spriteRenderer.flipX = in_faceLeft;
            _pulseAndSilhouetteMatOverrides.SetFlipX(in_faceLeft);
        }

        public void SetSilhouetteMaterialParameters(float in_distancePercentage, bool in_nextBeatStrong) {
            _pulseAndSilhouetteMatOverrides.SetSilhouetteDistance((1 - in_distancePercentage) * _silhouetteMatMaxDistance);
            
            float proximityAlpha = Mathf.Lerp(
                _silhouetteMatProximityAlphaRange.x, _silhouetteMatProximityAlphaRange.y, in_distancePercentage);
            _pulseAndSilhouetteMatOverrides.SetProximityAlpha(proximityAlpha);

            if (_nextBeatStrong != in_nextBeatStrong) {
                _nextBeatStrong = in_nextBeatStrong;
                LabeledColor silhouetteColors = in_nextBeatStrong ? 
                    _silhouetteStrongBeatColors : _silhouetteWeakBeatColors;

                _pulseAndSilhouetteMatOverrides.SetSilhouetteBaseColor(silhouetteColors.Color);
                _pulseAndSilhouetteMatOverrides.SetSilhouetteSecondaryColor(silhouetteColors.HDRColor);
            }
        }

        public void SetDashWindupTrigger()
        {
            ResetAnimationTriggers();
            _playerAnimator.SetTrigger(Constants.DashWindupClipName);
        }

        public void ToggleDashPreviewArrow(bool in_active) =>
            _dashPreviewArrow.gameObject.SetActive(in_active);

        public void UpdateDashPreviewArrowDirection(Vector2 in_direction) {
            float angle = Mathf.Atan2(in_direction.y, in_direction.x) * Mathf.Rad2Deg;
            _dashPreviewArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void ResetAnimationTriggers()
        {
            _playerAnimator.ResetTrigger(Constants.IdleClipName);
            _playerAnimator.ResetTrigger(Constants.RunClipName);
            _playerAnimator.ResetTrigger(Constants.LandClipName);
            _playerAnimator.ResetTrigger(Constants.RiseClipName);
            _playerAnimator.ResetTrigger(Constants.FallClipName);
            _playerAnimator.ResetTrigger(Constants.DashWindupClipName);
            _playerAnimator.ResetTrigger(Constants.DashStraightClipName);
            _playerAnimator.ResetTrigger(Constants.DashUpClipName);
            _playerAnimator.ResetTrigger(Constants.DashDownClipName);
            _playerAnimator.ResetTrigger(Constants.WallClingClipName);
            _playerAnimator.ResetTrigger(Constants.WallSlideClipName);
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
                    if (_characterMovement.DashDirection.y > 0)
                        _playerAnimator.SetTrigger(Constants.DashUpClipName);
                    else if (_characterMovement.DashDirection.y < 0)
                        _playerAnimator.SetTrigger(Constants.DashDownClipName);
                    else
                        _playerAnimator.SetTrigger(Constants.DashStraightClipName);
                    break;
                case CharacterState.WallCling:
                    _playerAnimator.SetTrigger(Constants.WallClingClipName);
                    break;
                case CharacterState.WallSlide:
                    _playerAnimator.SetTrigger(Constants.WallSlideClipName);
                    break;
            }
        }

        public void OnTogglePause(bool in_paused) => (this as IAnimatorPausable).ToggleAnimatorPause(in_paused);
    }
}
