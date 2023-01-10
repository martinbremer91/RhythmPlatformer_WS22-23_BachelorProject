using UnityEngine;

namespace GameplaySystems
{
    public class DashCrystal : CrystalBase
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            _characterStateController.CanDash = true;
        }
    }
}
