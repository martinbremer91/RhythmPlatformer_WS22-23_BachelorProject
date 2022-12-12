using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility_Scripts;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "LevelSequenceData", menuName = "Custom/Level Sequence Data")]
    public class LevelSequenceData : ScriptableObject
    {
        public string[] LevelSequence;

        public string GetLevelToLoadName()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            int currentIndexInLevelSequenceArray;

            if (LevelSequence.Any(l => l == currentSceneName))
            {
                currentIndexInLevelSequenceArray = 
                    Array.IndexOf(LevelSequence, currentSceneName);

                if (currentIndexInLevelSequenceArray + 1 >= LevelSequence.Length)
                    return Constants.MainMenu;
                else
                    return LevelSequence[currentIndexInLevelSequenceArray + 1];
            }
            
            Debug.LogWarning("Current Scene name not found in Level Sequence array. Returning Main Menu scene name");
            return Constants.MainMenu;
        }
    }
}