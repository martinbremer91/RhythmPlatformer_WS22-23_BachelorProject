using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public class CompanionSpriteController : MonoBehaviour, IInit<GameStateManager>, IAnimatorPausable {

    private CharacterStateController _characterStateController;
    [SerializeField] private VisualsData _companionVisualsData;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _companionAnimator;
    [SerializeField] private ParticleSystem _companionBodyParticles;
    [SerializeField] private ParticleSystem _companionTrailParticles;
    private PulseMaterialOverrides _pulseMaterialOverrides;

    private LabeledColor _defaultColors;
    private LabeledColor _noDashColors;

    private Gradient _defaultColorsGradient;
    private Gradient _noDashColorsGradient;

    public Animator Animator { 
        get => _companionAnimator; 
        set => _companionAnimator = value; 
    }

    private void OnDisable() => 
        _characterStateController.CanDashStateChanged -= UpdateCanDashColor;

    public void Init(GameStateManager in_gameStateManager) {
        _pulseMaterialOverrides = new PulseMaterialOverrides(_spriteRenderer);

        LabeledColor[] companionColors = _companionVisualsData.CompanionColors.ToArray();

        _defaultColors = 
            _companionVisualsData.GetColorByLabel(companionColors, "CompanionDefault");
        _noDashColors = 
            _companionVisualsData.GetColorByLabel(companionColors, "CompanionNoDash");

        _characterStateController = in_gameStateManager.CharacterStateController;
        in_gameStateManager.CharacterStateController.CanDashStateChanged += UpdateCanDashColor;

        _defaultColorsGradient = new Gradient();
        _defaultColorsGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(_defaultColors.Color, 0),
                new GradientColorKey(_defaultColors.Color, 1) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(0, 1)
            });

        _noDashColorsGradient = new Gradient();
        _noDashColorsGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(_noDashColors.Color, 0),
                new GradientColorKey(_noDashColors.Color, 1) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(0, 1)
            });
        
        UpdateCanDashColor(_characterStateController.CanDash);
    }

    public void UpdateCanDashColor(bool in_canDash) {
        LabeledColor colors = in_canDash ? _defaultColors : _noDashColors;
        Gradient gradient = in_canDash ? _defaultColorsGradient : _noDashColorsGradient;

        _pulseMaterialOverrides.SetBaseColor(colors.Color);
        _pulseMaterialOverrides.SetSecondaryColor(colors.HDRColor);

        ParticleSystem.ColorOverLifetimeModule bodyParticlesColorOverTime = 
            _companionBodyParticles.colorOverLifetime;
        ParticleSystem.ColorOverLifetimeModule trailParticlesColorOverTime = 
            _companionTrailParticles.colorOverLifetime;

        bodyParticlesColorOverTime.color = gradient;
        trailParticlesColorOverTime.color = gradient;
    }

    public void OnTogglePause(bool in_paused) => (this as IAnimatorPausable).ToggleAnimatorPause(in_paused);
}
