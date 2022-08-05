#if UNITY_EDITOR
using Gameplay;
using UnityEngine;

namespace Debug_and_Tools
{
    public class EditorReferenceCollector : MonoBehaviour
    {
        public static EditorReferenceCollector s_Instance;

        public CharacterMovement CharacterMovement;
        public CharacterStateController CharacterStateController;

        private void OnEnable()
        {
            if (s_Instance == null)
                s_Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
#endif
