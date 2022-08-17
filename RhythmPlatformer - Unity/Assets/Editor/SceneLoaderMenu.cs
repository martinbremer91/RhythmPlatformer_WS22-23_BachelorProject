using UnityEditor;
using UnityEditor.SceneManagement;
using Utility_Scripts;

namespace Editor
{
    public static class SceneLoaderMenu
    {
        [MenuItem("Scenes/Load Test Scene")]
        private static void LoadTestScene()
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/{Constants.TestScene}.unity");
        }
    }
}
