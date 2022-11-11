using Interfaces_and_Enums;
using UnityEngine;

namespace GameplaySystems
{
    public class Checkpoint : MonoBehaviour, IInit<LevelManager>
    {
        private LevelManager _levelManager;
        public Transform SpawnPoint;
        public bool SpawnFacingLeft;

        public void Init(LevelManager in_levelManager) => _levelManager = in_levelManager;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("Player"))
                _levelManager.SetCurrentCheckPoint(this);
        }
    }
}
