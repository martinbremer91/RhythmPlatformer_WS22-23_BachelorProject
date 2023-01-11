using GlobalSystems;
using UnityEngine;
using Scriptable_Object_Scripts;
using Utility_Scripts;
using System.Linq;

namespace UI_And_Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private UiManager _uiManager;
        [SerializeField] private LevelSequenceData _levelSequenceData;

        public void LoadLevel() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(GetFirstLevelName(), _uiManager));
        public void CloseGame() => Application.Quit();

        private string GetFirstLevelName()
        {
            if (_levelSequenceData == null || _levelSequenceData.LevelSequence == null || 
                !_levelSequenceData.LevelSequence.Any())
                return Constants.DemoLevel;

            return _levelSequenceData.LevelSequence[0];
        }
    }
}
