using UnityEngine;

namespace Utility_Scripts
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ScreenGradientController : MonoBehaviour
    {
        [SerializeField] private Material _gradientMaterial;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, _gradientMaterial);
        }
    }
}
