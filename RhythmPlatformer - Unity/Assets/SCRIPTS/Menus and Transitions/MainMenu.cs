using GlobalSystems;
using UnityEngine;
using Utility_Scripts;

namespace Menus_and_Transitions
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] UiManager _uiManager;

        public void LoadLevel() => 
            StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.DemoLevel, _uiManager));
        public void CloseGame() => Application.Quit();
    }
}
