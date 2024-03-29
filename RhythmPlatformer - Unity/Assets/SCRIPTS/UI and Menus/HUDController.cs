using UnityEngine;
using TMPro;
using Scriptable_Object_Scripts;
using GlobalSystems;
using Interfaces_and_Enums;

namespace UI_And_Menus {
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _beatDisplayText;

        public void InitializeHUD(GameStateManager in_gameStateManager, float in_beatLength, TrackData in_trackData) {
            gameObject.SetActive(in_gameStateManager.LoadedSceneType == SceneType.Level);
            _beatDisplayText.text = in_trackData.Meter.ToString();
        } 

        public void UpdateHUD(int in_beatTracker) =>
            _beatDisplayText.text = in_beatTracker.ToString();
    }
}
