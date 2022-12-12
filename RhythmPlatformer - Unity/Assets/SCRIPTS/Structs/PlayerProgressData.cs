using UnityEngine;
using UI_And_Menus;
using GlobalSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Structs
{
    [System.Serializable]
    public struct PlayerProgressData
    {
        public string CurrentLevelName;
        public int CurrentCheckpointIndex;

        public PlayerProgressData(string in_currentLevelName, int in_currentCheckpointIndex = -1) {
            CurrentLevelName = in_currentLevelName;
            CurrentCheckpointIndex = in_currentCheckpointIndex;
        }

        public async void SavePlayerProgressDataAsync(UiManager in_uiManager) {
            in_uiManager.SaveInProgressText.SetActive(true);
    #if UNITY_EDITOR
            string path = "Assets/JsonData/SaveData/PlayerProgress.json";
#else
            string path = Application.persistentDataPath + "/SaveData/PlayerProgress.json";
#endif
            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;

            string jsonData = JsonUtility.ToJson(this);
            await System.IO.File.WriteAllTextAsync(path, jsonData);

            if (CheckQuitFunction())
                return;

            in_uiManager.SaveInProgressText.SetActive(false);

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }
        }

        public bool LoadPlayerProgressData(ref PlayerProgressData ref_playerProgressData) {
            ref_playerProgressData = new PlayerProgressData(string.Empty);
    #if UNITY_EDITOR
            TextAsset userSoundPreferences =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/JsonData/SaveData/PlayerProgress.json");
    #else
                TextAsset userSoundPreferences =
                    Resources.Load<TextAsset>(Application.persistentDataPath + "/SaveData/PlayerProgress.json");
    #endif
            if (userSoundPreferences == null)
                return false;

            string jsonData = userSoundPreferences.text;
            this = JsonUtility.FromJson<PlayerProgressData>(jsonData);
            ref_playerProgressData = this;

            return true;
        }
    }
}
