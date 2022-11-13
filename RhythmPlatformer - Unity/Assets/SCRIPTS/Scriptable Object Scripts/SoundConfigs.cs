using Structs;
using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "Sound Configs", menuName = "Custom/Sound Configs")]
    public class SoundConfigs : ScriptableObject
    {
        [Header("Sound Preferences")] 
        public SoundPreferences SoundPreferences;
        [Space(10)] 
        [Header("Music Utilities Parameters")]
        public float MusicFadeDuration;
        [Space(10)]
        public float LowPassFilterFadeDuration;
        public float LowPassFilterFadeCutoffFrequency;
        
        public async void SaveSoundPreferences()
        {
            string jsonData = JsonUtility.ToJson(SoundPreferences);
            await System.IO.File.WriteAllTextAsync($"Assets/JsonData/SaveData/UserSoundPreferences.json", 
                jsonData);
        }

        public void LoadSoundPreferences(TextAsset in_userSoundPreferences)
        {
            string jsonData = in_userSoundPreferences.text;
            SoundPreferences = JsonUtility.FromJson<SoundPreferences>(jsonData);
        }
    }
}
