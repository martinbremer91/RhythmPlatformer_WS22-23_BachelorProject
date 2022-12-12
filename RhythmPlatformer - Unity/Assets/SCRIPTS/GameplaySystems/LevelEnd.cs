using System;
using Interfaces_and_Enums;
using UI_And_Menus;
using UnityEngine;
using Utility_Scripts;

namespace GlobalSystems
{
    public class LevelEnd : MonoBehaviour, IInit<GameStateManager>
    {
        private string _levelToLoadName;
        private string LevelToLoadName => 
            _levelToLoadName == String.Empty ? Constants.MainMenu : _levelToLoadName;

        private UiManager _uiManager;

        public void Init(GameStateManager in_gameStateManager)
        {
            _uiManager = in_gameStateManager.UiManager;
            _levelToLoadName = in_gameStateManager.LevelSequenceData.GetLevelToLoadName();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag(Constants.PlayerTag))
                LoadLevelToLoad();
        }

        private void LoadLevelToLoad() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(LevelToLoadName, _uiManager));
    }
}