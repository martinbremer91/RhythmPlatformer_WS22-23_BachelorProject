using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public abstract class CrystalBase : MonoBehaviour, IInit<GameStateManager>
{
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    protected CharacterStateController _characterStateController;
    protected bool _uncharged;

    public virtual void Init(GameStateManager in_gameStateManager) =>
            _characterStateController = in_gameStateManager.CharacterStateController;

    protected void HandleCharacterInTrigger()
    {
        _uncharged = true;
        _spriteRenderer.color = Color.gray;
        _characterStateController.BecomeGrounded += RechargeDashCrystal;
    }

    protected void RechargeDashCrystal()
    {
        _characterStateController.BecomeGrounded -= RechargeDashCrystal;
        _spriteRenderer.color = Color.cyan;
        _uncharged = false;
    }
}
