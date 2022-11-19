using UnityEngine;

namespace GameplaySystems
{
    public class DashCrystal : CrystalBase
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_uncharged || !collision.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            HandleCharacterInTrigger();
            _characterStateController.CanDash = true;
        }
    }
}
