using UnityEngine;

public class PulseMaterialOverrides
{
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _materialPropertyBlock;

    private float _outlineThicknessMax;
    public float OutlineThicknessMax {
        get {
            if (_materialPropertyBlock == null) {
                Debug.LogError("Trying to access material value before property block is set up");
                return 0;
            }

            if (_outlineThicknessMax == 0)
                _outlineThicknessMax = _materialPropertyBlock.GetFloat("_OutlineThicknessMax");

            return _outlineThicknessMax;
        }
    }

    public PulseMaterialOverrides(SpriteRenderer in_spriteRenderer)
    {
        _spriteRenderer = in_spriteRenderer;
        _materialPropertyBlock = new MaterialPropertyBlock();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    public void ChangeBaseColor(Color in_color)
    {
        _materialPropertyBlock.SetColor("_BaseColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void ChangeSecondaryColor(Color in_color) {
        _materialPropertyBlock.SetColor("_SecondaryColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetFlipX(bool in_flipX) {
        float value = in_flipX ? 1 : 0;
        _materialPropertyBlock.SetFloat("_FlipX", value);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSilhouetteDistance(float in_distance) {
        _materialPropertyBlock.SetFloat("Test", in_distance);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
