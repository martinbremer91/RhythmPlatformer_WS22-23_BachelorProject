using UnityEngine;

public class SilhouetteMaterialOverrides : PulseMaterialOverrides
{
    public SilhouetteMaterialOverrides(SpriteRenderer in_spriteRenderer) : base(in_spriteRenderer) {
        _spriteRenderer = in_spriteRenderer;
        _materialPropertyBlock = new MaterialPropertyBlock();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    public void SetFlipX(bool in_flipX) {
        float value = in_flipX ? 1 : 0;
        _materialPropertyBlock.SetFloat("_FlipX", value);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSilhouetteDistance(float in_distance) {
        _materialPropertyBlock.SetFloat("_OutlineThickness", in_distance);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetProximityAlpha(float in_alpha) {
        _materialPropertyBlock.SetFloat("_ProximityAlpha", in_alpha);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSilhouetteBaseColor(Color in_color) {
        _materialPropertyBlock.SetColor("_CenterColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSilhouetteSecondaryColor(Color in_color) {
        _materialPropertyBlock.SetColor("_EdgeColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
