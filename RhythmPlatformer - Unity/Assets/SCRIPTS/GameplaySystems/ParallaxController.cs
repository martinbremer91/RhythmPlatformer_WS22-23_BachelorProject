using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform _camTransform;
    [SerializeField] private Vector2 _parallaxAmount;

    private Vector2 _prevCamPosition;

    private void Start() =>
        _prevCamPosition = _camTransform.position;

    private void LateUpdate()
    {
        Vector2 camPosition = _camTransform.position;
        Vector2 camMovementDelta = camPosition - _prevCamPosition;

        transform.position += new Vector3(
            camMovementDelta.x * _parallaxAmount.x, 
            camMovementDelta.y * _parallaxAmount.y, 
            0);

        _prevCamPosition = camPosition;
    }
}
