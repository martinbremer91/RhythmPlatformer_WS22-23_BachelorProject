using System;
using Debug_and_Tools;
using Gameplay;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Editor
{
    public class CharacterStatusEditor : EditorWindow
    {
        private static CharacterStatusEditor s_instance;

        private CharacterStateController _characterStateController => 
            EditorReferenceCollector.s_Instance.CharacterStateController;
        private CharacterMovement _characterMovement => 
            EditorReferenceCollector.s_Instance.CharacterMovement;

        private Texture2D _unitCircleTexture;
        private Texture2D _directionalInputMarkerTexture;

        private readonly Rect _infoRect = new(new Vector2(5, 5), new Vector2(400, 400));
        
        private readonly Vector2 _inputStatePos = new(300, 10);
        private readonly Vector2 _markerOffset = new(45, 45);

        private InputState _inputState;
        private float _inputDeadzone;

        private bool _referencesCheck;

        #region INITIALIZATION

        [MenuItem("Debug/Character Status", true)]
        private static bool OpenWindowValidate() => s_instance == null;

        [MenuItem("Debug/Character Status")]
        private static void OpenWindow()
        {
            CharacterStatusEditor window = CreateWindow<CharacterStatusEditor>();
            window.titleContent = new GUIContent("Character Status");
        }

        private void OnEnable()
        {
            if (s_instance == null)
                s_instance = this;
            else
            {
                Close();
                return;
            }
            
            Init();
        }

        private void Init()
        {
            _unitCircleTexture = 
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Editor Resources/unit_circle.png");
            _directionalInputMarkerTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Editor Resources/input_direction_marker.png");
        }
        
        private void OnDisable() => s_instance = null;

        private bool CheckReferences()
        {
            if (_referencesCheck)
                return true;

            _referencesCheck =
                EditorReferenceCollector.s_Instance != null &&
                _characterMovement != null &&
                _characterStateController != null;

            return _referencesCheck;
        }

        #endregion

        private void OnGUI()
        {
            if (!CheckReferences())
            {
                GUI.Label(new Rect (_infoRect.position + new Vector2(5, 5), new Vector2(300, 20)), 
                    "Start Game to Refresh");
                return;
            }

            try
            {
                Draw();
                Repaint();
            }
            catch
            {
                _referencesCheck = false;
            }
        }

        private void Draw()
        {
            // Draw info box
            GUI.Box(_infoRect, String.Empty);
            
            // Draw Character State
            GUI.Label(new Rect (_infoRect.position + new Vector2(5, 5), new Vector2(300, 20)), 
                "Character State: " + 
                Enum.GetName(typeof(CharacterState), _characterStateController.CurrentCharacterState));
            
            DrawVelocities();
            DrawInputState();
        }

        private void DrawVelocities()
        {
            GUI.Label(new Rect (_infoRect.position + new Vector2(5, 25), new Vector2(300, 20)), 
                "Character Velocity: " + _characterMovement.CharacterVelocity);
            GUI.Label(new Rect (_infoRect.position + new Vector2(25, 40), new Vector2(300, 20)), 
                "Run Velocity: " + _characterMovement.RunVelocity);
            GUI.Label(new Rect (_infoRect.position + new Vector2(25, 55), new Vector2(300, 20)), 
                "Fall Velocity: " + _characterMovement.FallVelocity);
            GUI.Label(new Rect (_infoRect.position + new Vector2(25, 70), new Vector2(300, 20)), 
                "Rise Velocity: " + _characterMovement.RiseVelocity);
            GUI.Label(new Rect (_infoRect.position + new Vector2(25, 85), new Vector2(300, 20)), 
                "Land Velocity: " + _characterMovement.LandVelocity);
            GUI.Toggle(new Rect(_infoRect.position + new Vector2(5, 105),
                    new Vector2(200, 20)), 
                _characterStateController.CanWallCling, "Wall Cling available");
            GUI.Label(new Rect (_infoRect.position + new Vector2(25, 120), new Vector2(300, 20)), 
                "Wall Cling Timer: " + $"{_characterStateController.WallClingTimer:N2}");
        }

        private void DrawInputState()
        {
            Vector2 currentInput = new Vector2(_inputState.DirectionalInput.x, 
                                   -_inputState.DirectionalInput.y).normalized * 
                               _inputState.DirectionalInput.magnitude * 50;

            GUI.DrawTexture(new Rect(_inputStatePos, new Vector2(100, 100)),
                _unitCircleTexture);
            GUI.DrawTexture(new Rect(_inputStatePos + _markerOffset + currentInput, 
                    new(10, 10)), _directionalInputMarkerTexture);
            
            GUI.Label(new Rect (_inputStatePos + new Vector2(0, 105), new Vector2(300, 20)), 
                _inputState.DirectionalInput.ToString());
            GUI.Toggle(new Rect(_inputStatePos + new Vector2(0, 120),
                    new Vector2(200, 20)), 
                _inputState.DashButton == InputActionPhase.Performed, "DASH");
        }
    }
}
