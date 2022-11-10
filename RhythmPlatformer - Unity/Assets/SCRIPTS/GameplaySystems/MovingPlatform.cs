using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class MovingPlatform : MonoBehaviour
    {
        public static bool test;
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.enabled && col.gameObject.CompareTag("Player"))
            {
                test = true;
                GameStateManager.s_Instance.CharacterStateController.CurrentCharacterState = CharacterState.Land;
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            test = false;
        }
    }
}
