using Scriptable_Object_Scripts;
using UnityEngine;

namespace Systems
{
    public class GameplayReferenceManager : MonoBehaviour
    {
        public static GameplayReferenceManager s_Instance;

        [Header("CONFIGS")]
        public MovementConfigs MovementConfigs;
        public ControlSettings ControlSettings;

        private void OnEnable()
        {
            if (s_Instance == null)
                s_Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
