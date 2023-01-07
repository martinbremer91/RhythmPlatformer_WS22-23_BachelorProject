using GameplaySystems;
using GlobalSystems;
using UnityEngine;
using UnityEngine.UI;

namespace UI_And_Menus
{
    public class CheckpointsMenu : MonoBehaviour
    {
        private GameStateManager _gameStateManager;
        private UiManager _uiManager;
        private LevelManager _levelManager;

        [SerializeField] private Transform _verticalGroup;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Button _backButton;

        private Checkpoint[] _checkpoints;
        private LevelEnd _levelEnd;

        [HideInInspector] public GameObject DefaultSelectedButton;

        private Vector3 _beginningPos;

        public void SetCheckpoints(Checkpoint[] in_checkpoints) => _checkpoints = in_checkpoints;
        public void SetLevelEnd(LevelEnd in_levelEnd, GameStateManager in_gameStateManager, LevelManager in_levelManager)
        {
            // NOTE: this logic assumes Checkpoints are initialized before LevelEnd by LevelManager

            _levelEnd = in_levelEnd;

            _beginningPos = in_gameStateManager.CharacterStateController.transform.position;

            _gameStateManager = in_gameStateManager;
            _uiManager = in_gameStateManager.UiManager;
            _levelManager = in_levelManager;
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            foreach (Transform child in _verticalGroup)
                Destroy(child.gameObject);

            Button[] buttons = new Button[_checkpoints.Length + 3];

            int counter = 0;

            GameObject beginningButtonGO = Instantiate(_buttonPrefab, _verticalGroup);
            beginningButtonGO.GetComponentInChildren<TMPro.TMP_Text>().text = "BEGINNING";
            buttons[counter] = beginningButtonGO.GetComponent<Button>();
            buttons[counter].onClick.AddListener(HandleBeginningSelected);
            counter++;

            foreach (Checkpoint checkpoint in _checkpoints)
            {
                GameObject buttonGO = Instantiate(_buttonPrefab, _verticalGroup);
                buttonGO.GetComponentInChildren<TMPro.TMP_Text>().text = "CHECKPOINT " + counter;
                buttons[counter] = buttonGO.GetComponent<Button>();
                buttons[counter].onClick.AddListener(() => HandleCheckpointSelected(checkpoint));

                counter++;
            }

            GameObject levelEndButtonGO = Instantiate(_buttonPrefab, _verticalGroup);
            levelEndButtonGO.GetComponentInChildren<TMPro.TMP_Text>().text = "LEVEL END";
            buttons[counter] = levelEndButtonGO.GetComponent<Button>();
            buttons[counter].onClick.AddListener(HandleLevelEndSelected);

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

        private void HandleBeginningSelected()
        {
            _levelManager.ResetCheckpoints();

            _levelManager.SetSpawnPointToPosition(_beginningPos);
            _gameStateManager.CharacterStateController.transform.position = _beginningPos;
            CloseMenu();
        }

        private void HandleCheckpointSelected(Checkpoint in_checkpoint)
        {
            _levelManager.ResetCheckpoints();

            in_checkpoint.CheckpointTouched = true;
            _levelManager.SetCurrentCheckPoint(in_checkpoint);
            in_checkpoint.UpdateCheckpointVisuals();

            _gameStateManager.CharacterStateController.transform.position = in_checkpoint.SpawnPoint.position;
            CloseMenu();
        }

        private void HandleLevelEndSelected()
        {
            _gameStateManager.CharacterStateController.transform.position = _levelEnd.transform.position;
            CloseMenu();
        }

        private void CloseMenu()
        {
            gameObject.SetActive(false);
            _gameStateManager.ScheduleTogglePause();
        }
    }
}

