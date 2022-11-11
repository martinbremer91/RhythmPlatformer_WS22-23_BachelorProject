using Gameplay;
using Interfaces_and_Enums;
using UnityEngine;

public class deleteme : MonoBehaviour
{
    public CharacterStateController test;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player") || test.CurrentCharacterState == CharacterState.Dead)
            return;
        
        test.DieAsync();
    }
}
