using GlobalSystems;
using Interfaces_and_Enums;
using System;
using UnityEngine;
using Utility_Scripts;

namespace GameplaySystems
{
    public class KeyCrystal : CrystalBase, IInit<GameStateManager>
    {
        private GameStateManager _gameStateManager;

        [SerializeField] private GameObject _graphicsParent;
        [SerializeField] private BoxCollider2D _trigger;

        public Action KeyTouched;
        public Action KeyReset;

        public override void Init(GameStateManager in_gameStateManager)
        {
            _gameStateManager = in_gameStateManager;
            in_gameStateManager.CharacterStateController.Respawn += ResetKey;
        }

        private void OnDisable()
        {
            _gameStateManager.CharacterStateController.Respawn -= ResetKey;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(Constants.PlayerTag))
            {
                KeyTouched?.Invoke();
                ToggleGraphicsAndTrigger(false);
            }
        }

        private void ResetKey()
        {
            KeyReset?.Invoke();
            ToggleGraphicsAndTrigger(true);
        }

        private void ToggleGraphicsAndTrigger(bool in_active)
        {
            _graphicsParent.SetActive(in_active);
            _trigger.enabled = in_active;
        }
    }
}
