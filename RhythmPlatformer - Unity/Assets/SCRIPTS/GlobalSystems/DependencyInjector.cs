using Interfaces_and_Enums;
using UnityEngine;

namespace GlobalSystems
{
    public abstract class DependencyInjector : MonoBehaviour, IInit<GameStateManager>
    {
        [SerializeField] private GameStateManager _gameStateManager;
        
        private SceneType SceneType => GetSceneType();
        protected abstract SceneType GetSceneType();

        private UpdateType UpdateType => GetUpdateType();
        protected abstract UpdateType GetUpdateType();

        private void OnEnable()
        {
            if (GameStateManager.s_Instance == null)
                GameStateManager.s_Instance = _gameStateManager;
            else
                _gameStateManager = GameStateManager.s_Instance;

            _gameStateManager.Init(this);
        }

        public virtual void Init(GameStateManager in_gameStateManager)
        {
            GameStateManager.s_LoadedSceneType = SceneType;
            GameStateManager.s_ActiveUpdateType = UpdateType;
            Destroy(gameObject);
        }
    }
}
