using GameplaySystems;
using UnityEngine;
using UnityEngine.UI;

namespace UI_And_Menus
{
    public class CheckpointsMenu : MonoBehaviour
    {
        [SerializeField] private Transform _verticalGroup;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Button _backButton;

        private Checkpoint[] _checkpoints;
        private LevelEnd _levelEnd;

        [HideInInspector] public GameObject DefaultSelectedButton;

        public void SetCheckpoints(Checkpoint[] in_checkpoints) => _checkpoints = in_checkpoints;
        public void SetLevelEnd(LevelEnd in_levelEnd)
        {
            // NOTE: this logic assumes Checkpoints are initialized before LevelEnd by LevelManager

            _levelEnd = in_levelEnd;
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            Button[] buttons = new Button[_checkpoints.Length + 2];

            int counter = 0;

            foreach (Checkpoint checkpoint in _checkpoints)
            {
                GameObject buttonGO = Instantiate(_buttonPrefab, _verticalGroup);
                buttonGO.GetComponentInChildren<TMPro.TMP_Text>().text = "CHECKPOINT " + (counter + 1).ToString();
                buttons[counter] = buttonGO.GetComponent<Button>();

                counter++;
            }

            GameObject levelEndButtonGO = Instantiate(_buttonPrefab, _verticalGroup);
            levelEndButtonGO.GetComponentInChildren<TMPro.TMP_Text>().text = "Level End";
            buttons[counter] = levelEndButtonGO.GetComponent<Button>();

            counter++;
            buttons[counter] = _backButton;

            for (int i = 0; i < buttons.Length; i++)
            {
                int prevIndex = i - 1 >= 0 ? i - 1 : buttons.Length - 1;
                int nextIndex = i + 1 < buttons.Length ? i + 1 : 0;

                Navigation buttonNavigation = buttons[i].navigation;
                buttonNavigation.mode = Navigation.Mode.Explicit;
                buttonNavigation.selectOnDown = buttons[nextIndex];
                buttonNavigation.selectOnUp = buttons[prevIndex];
                buttons[i].navigation = buttonNavigation;
            }

            DefaultSelectedButton = buttons[0].gameObject;
        }
    }
}

