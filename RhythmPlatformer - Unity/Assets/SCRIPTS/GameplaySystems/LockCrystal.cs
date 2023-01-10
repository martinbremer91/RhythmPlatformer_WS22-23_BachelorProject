using UnityEngine;

namespace GameplaySystems
{
    public class LockCrystal : MonoBehaviour
    {
        [SerializeField] private KeyCrystal _key;

        [SerializeField] private GameObject _graphicsParent;
        [SerializeField] private BoxCollider2D _collider;

        private void OnEnable()
        {
            _key.KeyTouched += Unlock;
            _key.KeyReset += Lock;
        }

        private void OnDisable()
        {
            _key.KeyTouched -= Unlock;
            _key.KeyReset -= Lock;
        }

        private void Lock() => ToggleGraphicsAndCollider(true);

        private void Unlock() => ToggleGraphicsAndCollider(false);

        private void ToggleGraphicsAndCollider(bool in_active)
        {
            _graphicsParent.SetActive(in_active);
            _collider.enabled = in_active;
        }
    }
}
