using Gameplay;
using GameplaySystems;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public class Hazard : MonoBehaviour, IInit<GameStateManager>
{
    [HideInInspector] public CharacterStateController _characterStateController;

    [SerializeField] private MovementRoutine _movementRoutine;

    public void Init(GameStateManager in_gameStateManager)
    {
        _characterStateController = in_gameStateManager.CharacterStateController;

        if (_movementRoutine != null)
            _movementRoutine.Init(in_gameStateManager);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player") || _characterStateController.Dead)
            return;
        
        _characterStateController.DieAsync();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player") || _characterStateController.Dead)
            return;

        _characterStateController.DieAsync();
    }
}
