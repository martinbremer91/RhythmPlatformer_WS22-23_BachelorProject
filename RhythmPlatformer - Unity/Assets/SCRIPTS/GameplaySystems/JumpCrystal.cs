using Gameplay;
using GlobalSystems;
using UnityEngine;

public class JumpCrystal : CrystalBase
{
    private CharacterInput _characterInput;

    public override void Init(GameStateManager in_gameStateManager)
    {
        base.Init(in_gameStateManager);
        _characterInput = in_gameStateManager.CharacterInput;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _characterStateController.Dead)
            return;

        _characterInput.InputState.JumpCommand = true;
    }
}
