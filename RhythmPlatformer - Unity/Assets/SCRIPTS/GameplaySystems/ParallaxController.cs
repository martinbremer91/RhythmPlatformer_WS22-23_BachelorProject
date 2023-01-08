using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private Transform _camTransform;
    [SerializeField, Range(0, 1)] private float _parallaxAmountX;
    [SerializeField, Range(0, 1)] private float _parallaxAmountY;

    private Vector2 _prevCamPosition;

    private void Start()
    {
        _camTransform = Camera.main.transform;
        _prevCamPosition = _camTransform.position;
    }

    private void LateUpdate()
    {
        Vector2 camPosition = _camTransform.position;
        Vector2 camMovementDelta = camPosition - _prevCamPosition;

        transform.position += new Vector3(
            camMovementDelta.x * _parallaxAmountX, 
            camMovementDelta.y * _parallaxAmountY, 
            0);

        _prevCamPosition = camPosition;
    }
}
