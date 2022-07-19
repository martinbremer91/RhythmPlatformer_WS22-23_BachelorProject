using UnityEngine;

namespace Systems
{
    public class ReferenceManager : MonoBehaviour
    {
        public static ReferenceManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
