using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

[System.Serializable]
public struct PlayerProgressData
{
    public string CurrentLevelName;
    public int CurrentCheckpointIndex;

    public PlayerProgressData(string in_currentLevelName, int in_currentCheckpointIndex = -1) {
        CurrentLevelName = in_currentLevelName;
        CurrentCheckpointIndex = in_currentCheckpointIndex;
    }

    public async void SavePlayerProgressDataAsync() {
#if UNITY_EDITOR
        string path = "Assets/JsonData/SaveData/PlayerProgress.json";
#else
        string path = Application.persistentDataPath + "/SaveData/PlayerProgress.json";
#endif
        string jsonData = JsonUtility.ToJson(this);
        await System.IO.File.WriteAllTextAsync(path, jsonData);
    }

    public bool LoadPlayerProgressData(out PlayerProgressData out_playerProgressData) {
        out_playerProgressData = new PlayerProgressData(string.Empty);
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
        out_playerProgressData = this;

        return true;
    }
}
