using Gameplay;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class DashCrystal : MonoBehaviour, IInit<CharacterStateController>
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private CharacterStateController _characterStateController;
        private bool _uncharged;

        public void Init(CharacterStateController in_characterStateController) =>
            _characterStateController = in_characterStateController;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_uncharged || !collision.gameObject.CompareTag("Player") || _characterStateController.Dead)
                return;

            _uncharged = true;
            _spriteRenderer.color = Color.gray;

            _characterStateController.CanDash = true;
            _characterStateController.BecomeGrounded += Recharge;
        }

        private void Recharge()
        {
            _characterStateController.BecomeGrounded -= Recharge;
            _spriteRenderer.color = Color.cyan;
            _uncharged = false;
        }
    }
}
