using Gameplay;
using UnityEngine;

public class deleteme : MonoBehaviour
{
    public CharacterStateController test;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player") || test.Dead)
            return;
        
        test.DieAsync();
    }
}
