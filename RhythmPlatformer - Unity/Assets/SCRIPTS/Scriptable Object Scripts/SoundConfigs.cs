using Structs;
using UnityEngine;
using GlobalSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Sound Configs", menuName = "Custom/Sound Configs")]
    public class SoundConfigs : ScriptableObject
    {
        public SoundPreferencesData SoundPreferences;
        [Space(10)] 
        [Header("Music Utilities Parameters")]
        public float MusicFadeDuration;
        [Space(10)]
        public float LowPassFilterFadeDuration;
        public float LowPassFilterFadeCutoffFrequency;
        
        public async void SaveSoundPreferencesAsync(UiManager in_uiManager)
        {
            in_uiManager.SaveInProgressText.SetActive(true);
#if UNITY_EDITOR
            string path = "Assets/JsonData/SaveData/UserSoundPreferences.json";
#else
            string path = Application.persistentDataPath + "/SaveData/UserSoundPreferences.json";
#endif
            string jsonData = JsonUtility.ToJson(SoundPreferences);
            await System.IO.File.WriteAllTextAsync(path, jsonData);

            in_uiManager.SaveInProgressText.SetActive(false);
        }

        public bool LoadSoundPreferences()
        {
#if UNITY_EDITOR
            TextAsset userSoundPreferences =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/JsonData/SaveData/UserSoundPreferences.json");
#else
            TextAsset userSoundPreferences =
                Resources.Load<TextAsset>(Application.persistentDataPath + "/SaveData/UserSoundPreferences.json");
#endif
            if (userSoundPreferences == null)
                return false;
            
            string jsonData = userSoundPreferences.text;
            SoundPreferences = JsonUtility.FromJson<SoundPreferencesData>(jsonData);

            return true;
        }
    }
}
