#if UNITY_EDITOR
using Gameplay;
using Systems;
using UnityEngine;

namespace Debug_and_Tools
{
    public class EditorReferenceCollector : MonoBehaviour
    {
        public static EditorReferenceCollector s_Instance;

        public BeatManager BeatManager;
        
        public CharacterMovement CharacterMovement;
        public CharacterStateController CharacterStateController;
        public CharacterInput CharacterInput;

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
