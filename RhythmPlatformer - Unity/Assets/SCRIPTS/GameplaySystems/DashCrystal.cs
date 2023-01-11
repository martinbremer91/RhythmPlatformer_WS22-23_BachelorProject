using UnityEngine;

namespace GameplaySystems
{
    public class DashCrystal : CrystalBase
    {
        [SerializeField] private ParticleSystem _burstParticleSystem;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player") || _characterStateController.CanDash || _characterStateController.Dead)
                return;

            _burstParticleSystem.Play();
            _characterStateController.CanDash = true;
        }
    }
}
