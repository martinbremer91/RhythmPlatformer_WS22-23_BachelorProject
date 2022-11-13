using Structs;
using UnityEditor;
using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Sound Configs", menuName = "Custom/Sound Configs")]
    public class SoundConfigs : ScriptableObject
    {
        public SoundPreferences SoundPreferences;
        [Space(10)] 
        [Header("Music Utilities Parameters")]
        public float MusicFadeDuration;
        [Space(10)]
        public float LowPassFilterFadeDuration;
        public float LowPassFilterFadeCutoffFrequency;
        
        public async void SaveSoundPreferencesAsync()
        {
            string jsonData = JsonUtility.ToJson(SoundPreferences);
            await System.IO.File.WriteAllTextAsync("Assets/JsonData/SaveData/UserSoundPreferences.json", 
                jsonData);
        }

        public bool LoadSoundPreferences()
        {
            TextAsset userSoundPreferences =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/JsonData/SaveData/UserSoundPreferences.json");

            if (userSoundPreferences == null)
                return false;
            
            string jsonData = userSoundPreferences.text;
            SoundPreferences = JsonUtility.FromJson<SoundPreferences>(jsonData);
            Debug.Log("load");
            
            return true;
        }
    }
}
