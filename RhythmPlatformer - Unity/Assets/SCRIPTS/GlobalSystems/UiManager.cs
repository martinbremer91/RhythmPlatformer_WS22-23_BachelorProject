using Interfaces_and_Enums;
using UnityEngine;
using Utility_Scripts;

namespace GlobalSystems
{
    public class UiManager : MonoBehaviour, IRefreshable, IInit
    {
        private static UiManager s_Instance;
        
        [SerializeField] private GameObject _menuUI;
        [SerializeField] private GameObject _gameplayUI;
        
        public GameObject PlayIcon;
        public GameObject RecordIcon;

        private void OnEnable()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            (this as IRefreshable).RegisterRefreshable();
        } 
        
        private void OnDisable() => (this as IRefreshable).DeregisterRefreshable();

        public void Init()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        public void SceneRefresh()
        {
            SceneType currentSceneType = GameStateManager.s_LoadedSceneType;

            _menuUI.SetActive(currentSceneType == SceneType.MainMenu);
            _gameplayUI.SetActive(currentSceneType == SceneType.Level);
        }
        
        public void LoadMainMenu() => StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.MainMenu));
    }
}
