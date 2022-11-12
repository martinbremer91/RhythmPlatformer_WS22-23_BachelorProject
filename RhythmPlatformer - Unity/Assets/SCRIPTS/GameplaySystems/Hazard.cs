using Gameplay;
using Interfaces_and_Enums;
using UnityEngine;

public class Hazard : MonoBehaviour, IInit<CharacterStateController>
{
    [HideInInspector] public CharacterStateController _characterStateController;

    public void Init(CharacterStateController in_characterStateController) =>
        _characterStateController = in_characterStateController;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player") || _characterStateController.Dead)
            return;
        
        _characterStateController.DieAsync();
    }
}
