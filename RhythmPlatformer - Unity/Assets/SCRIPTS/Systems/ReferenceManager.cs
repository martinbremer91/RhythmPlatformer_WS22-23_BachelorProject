using Gameplay;
using UnityEngine;

namespace Systems
{
    public class ReferenceManager : MonoBehaviour
    {
        public static ReferenceManager Instance;

        #region REFERENCES

        public CharacterInput CharacterInput;
        public CharacterMovement CharacterMovement;
        public CharacterSpriteController CharacterSpriteController;
        public CharacterStateController CharacterStateController;

        #endregion

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
