using UnityEditor;
using UnityEditor.SceneManagement;
using Utility_Scripts;

namespace Editor
{
    public static class SceneLoaderMenu
    {
        [MenuItem("Scenes/Main Menu")]
        private static void LoadMainMenu() =>
            EditorSceneManager.OpenScene($"Assets/Scenes/{Constants.MainMenu}.unity");

        [MenuItem("Scenes/SCENES FOLDER")]
        private static void SelectScenesFolder() =>
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Scenes", typeof(UnityEngine.Object)));

        [MenuItem("Scenes/LEVELS FOLDER")]
        private static void SelectLevelsFolder() =>
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Scenes/Game Levels",
                typeof(UnityEngine.Object)));

        [MenuItem("Scenes/Utility Scenes/Test Level")]
        private static void LoadTestLevel() =>
            EditorSceneManager.OpenScene($"Assets/Scenes/Utility Scenes/{Constants.TestLevel}.unity");
        
        [MenuItem("Scenes/Utility Scenes/Template Level")]
        private static void LoadTemplateLevel() =>
            EditorSceneManager.OpenScene($"Assets/Scenes/Utility Scenes/{Constants.TemplateLevel}.unity");
    }
}
