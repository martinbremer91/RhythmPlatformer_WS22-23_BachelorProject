using UnityEditor;
using UnityEditor.SceneManagement;
using Utility_Scripts;

namespace Editor
{
    public static class SceneLoaderMenu
    {
        [MenuItem("Scenes/Main Menu")]
        private static void LoadMainMenu()
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{Constants.MainMenu}.unity");
        }
        
        [MenuItem("Scenes/Test Level")]
        private static void LoadTestLevel()
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{Constants.TestLevel}.unity");
        }
    }
}
