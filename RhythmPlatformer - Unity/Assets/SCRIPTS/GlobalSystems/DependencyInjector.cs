using Interfaces_and_Enums;
using UnityEngine;

namespace GlobalSystems
{
    public abstract class DependencyInjector : MonoBehaviour, IInit<GameStateManager>
    {
        [SerializeField] private GameStateManager _gameStateManager;
        
        private SceneType SceneType => GetSceneType();
        protected abstract SceneType GetSceneType();

        private void OnEnable()
        {
            if (GameStateManager.s_Instance == null)
                GameStateManager.s_Instance = _gameStateManager;

            _gameStateManager.Init(this);
        }

        public virtual void Init(GameStateManager in_gameStateManager)
        {
            in_gameStateManager.LoadedSceneType = SceneType;
            Destroy(gameObject);
        }
    }
}
