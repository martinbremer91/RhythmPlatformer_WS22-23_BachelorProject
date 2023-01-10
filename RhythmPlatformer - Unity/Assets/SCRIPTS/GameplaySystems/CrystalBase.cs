using Gameplay;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

public abstract class CrystalBase : MonoBehaviour, IInit<GameStateManager>
{
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    protected CharacterStateController _characterStateController;

    public virtual void Init(GameStateManager in_gameStateManager) =>
        _characterStateController = in_gameStateManager.CharacterStateController;
}
