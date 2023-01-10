using Gameplay;
using GameplaySystems;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public class Hazard : MonoBehaviour, IInit<GameStateManager>
{
    [HideInInspector] public CharacterStateController _characterStateController;

    [SerializeField] private MovementRoutine _movementRoutine;

    [SerializeField] private bool _slidingIsInvicible;

    public void Init(GameStateManager in_gameStateManager)
    {
        _characterStateController = in_gameStateManager.CharacterStateController;

        if (_movementRoutine != null)
            _movementRoutine.Init(in_gameStateManager);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player") || _characterStateController.Dead)
            return;

        if (_slidingIsInvicible && _characterStateController.Sliding)
            return;

        _characterStateController.DieAsync();
    }
}
