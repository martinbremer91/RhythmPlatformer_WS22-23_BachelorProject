using Interfaces_and_Enums;
using Scriptable_Object_Scripts;
using UI_And_Menus;
using UnityEngine;

namespace GlobalSystems
{
    public abstract class DependencyInjector : MonoBehaviour, IInit<GameStateManager>
    {
        [SerializeField] private GameStateManager _gameStateManager;
        [SerializeField] private TrackData _trackData;
        [SerializeField] private HUDController _hudController;
        
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
            _gameStateManager.CurrentTrackData = _trackData;
            _gameStateManager.LoadedSceneType = SceneType;
            _gameStateManager.ActiveUpdateType = UpdateType;
            _gameStateManager.HUDController = _hudController;
            Destroy(gameObject);
        }
    }
}
