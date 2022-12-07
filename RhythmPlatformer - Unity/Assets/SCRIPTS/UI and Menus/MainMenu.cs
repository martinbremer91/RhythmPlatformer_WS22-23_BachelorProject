using GlobalSystems;
using UnityEngine;
using Utility_Scripts;

namespace UI_And_Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] UiManager _uiManager;

        public void LoadLevel() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.DemoLevel, _uiManager));
        public void CloseGame() => Application.Quit();
    }
}
