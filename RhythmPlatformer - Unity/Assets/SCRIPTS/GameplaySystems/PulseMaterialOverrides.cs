using UnityEngine;

public class PulseMaterialOverrides
{
    protected SpriteRenderer _spriteRenderer;
    protected MaterialPropertyBlock _materialPropertyBlock;

    public PulseMaterialOverrides(SpriteRenderer in_spriteRenderer)
    {
        _spriteRenderer = in_spriteRenderer;
        _materialPropertyBlock = new MaterialPropertyBlock();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    public void SetBaseColor(Color in_color)
    {
        _materialPropertyBlock.SetColor("_BaseColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSecondaryColor(Color in_color) {
        _materialPropertyBlock.SetColor("_SecondaryColor", in_color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
