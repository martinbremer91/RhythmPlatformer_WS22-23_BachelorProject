using GlobalSystems;
using UnityEngine;
using TMPro;
using Interfaces_and_Enums;

namespace UI_And_Menus
{
    public class PauseMenu : MonoBehaviour, IInit<GameStateManager>
    {
        private GameStateManager _gameStateManager;

        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private TMP_Text _pausedBeatText;
        [SerializeField] private TMP_Text _countInText;

        public void Init(GameStateManager in_gameStateManager) => _gameStateManager = in_gameStateManager;

        public void TogglePauseMenu(bool in_paused) {
            _pausePanel.SetActive(in_paused);
            _gameStateManager.UiManager.HandleOpenPauseMenu();
        }

        public void ResumeButton() => _gameStateManager.ScheduleTogglePause();

        public void SetPausedBeatText(int in_value) => _pausedBeatText.text = in_value.ToString();

        public void SetCountInText(int in_value, bool in_active = true)
        {
            if (!in_active)
            {
                _countInText.enabled = false;
                return;
            }
            else if (!_countInText.isActiveAndEnabled)
                _countInText.enabled = true;

            _countInText.text = in_value.ToString();
        }
    }
}
