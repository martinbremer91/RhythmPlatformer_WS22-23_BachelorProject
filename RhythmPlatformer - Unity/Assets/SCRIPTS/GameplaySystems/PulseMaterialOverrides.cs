using UnityEngine;

public class PulseMaterialOverrides : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    float test;

    private void Update()
    {
        test += Time.deltaTime;

        if (test >= 2)
        {
            test = 0;

            System.Random rnd = new System.Random();

            float r = rnd.Next(1, 100) * .01f;
            float g = rnd.Next(1, 100) * .01f;
            float b = rnd.Next(1, 100) * .01f;

            _materialPropertyBlock.SetColor("_BaseColor", new Color(r, g, b, 1));
            _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}
