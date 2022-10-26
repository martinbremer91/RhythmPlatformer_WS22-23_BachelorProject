using GlobalSystems;
using UnityEngine;
using Utility_Scripts;

namespace Menus_and_Transitions
{
    public class MainMenu : MonoBehaviour
    {
        public void LoadLevel() => StartCoroutine(SceneLoadManager.LoadSceneCoroutine(Constants.TestLevel));
        public void CloseGame() => Application.Quit();
    }
}
