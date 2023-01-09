using UnityEngine;
using TMPro;
using Scriptable_Object_Scripts;

namespace UI_And_Menus {
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _beatDisplayText;

        public void InitializeHUD(float in_beatLength, TrackData in_trackData) => 
            _beatDisplayText.text = in_trackData.Meter.ToString();

        public void UpdateHUD(int in_beatTracker) =>
            _beatDisplayText.text = in_beatTracker.ToString();
    }
}
