using GlobalSystems;
using UnityEngine;

namespace Menus_and_Transitions
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        public void TogglePauseMenu(bool in_paused) => _pausePanel.SetActive(in_paused);

        public void ResumeButton() => GameStateManager.s_Instance.ScheduleTogglePause();
    }
}
