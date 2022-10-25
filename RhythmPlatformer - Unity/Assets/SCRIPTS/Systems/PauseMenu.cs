using UnityEngine;

namespace Systems
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        public void TogglePauseMenu(bool in_paused) => _pausePanel.SetActive(in_paused);
    }
}
