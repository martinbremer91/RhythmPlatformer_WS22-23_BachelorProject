using System;
using Gameplay;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Editor
{
    public class CharacterStatusEditor : EditorWindow
    {
        private static CharacterStatusEditor Instance;

        private Texture2D unitCircleTexture;
        private Texture2D directionalInputMarkerTexture;

        private readonly Rect infoRect = new(new Vector2(5, 5), new Vector2(400, 400));
        
        private readonly Vector2 inputStatePos = new(300, 10);
        private readonly Vector2 markerOffset = new(45, 45);

        private float inputDeadzone;

        #region INITIALIZATION

        [MenuItem("Debug/Character Status", true)]
        private static bool OpenWindowValidate() => Instance == null;

        [MenuItem("Debug/Character Status")]
        private static void OpenWindow()
        {
            CharacterStatusEditor window = CreateWindow<CharacterStatusEditor>();
            window.titleContent = new GUIContent("Character Status");
        }

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Close();
                return;
            }
            
            Init();
        }

        private void Init()
        {
            unitCircleTexture = 
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Editor Resources/unit_circle.png");
            directionalInputMarkerTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Editor Resources/input_direction_marker.png");
        }
        
        private void OnDisable() => Instance = null;

        #endregion

        private void OnGUI()
        {
            Draw();
            Repaint();
        }

        private void Draw()
        {
            // Draw info box
            GUI.Box(infoRect, String.Empty);
            
            // Draw Character State
            GUI.Label(new Rect (infoRect.position + new Vector2(5, 5), new Vector2(300, 20)), 
                "Character State: " + 
                Enum.GetName(typeof(CharacterState), CharacterStateController.CurrentCharacterState));
            
            DrawVelocities();

            DrawInputState();
        }

        private void DrawVelocities()
        {
            GUI.Label(new Rect (infoRect.position + new Vector2(5, 25), new Vector2(300, 20)), 
                "Character Velocity: " + CharacterMovement.CharacterVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 40), new Vector2(300, 20)), 
                "Run Velocity: " + CharacterMovement.RunVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 55), new Vector2(300, 20)), 
                "Fall Velocity: " + CharacterMovement.FallVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 70), new Vector2(300, 20)), 
                "Rise Velocity: " + CharacterMovement.RiseVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 85), new Vector2(300, 20)), 
                "Air Drift Velocity: " + CharacterMovement.AirDriftVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 100), new Vector2(300, 20)), 
                "Land Velocity: " + CharacterMovement.LandVelocity);
        }

        private void DrawInputState()
        {
            Vector2 currentInput = 
                new Vector2(CharacterInput.InputState.DirectionalInput.x, 
                    -CharacterInput.InputState.DirectionalInput.y).normalized * 
                CharacterInput.InputState.DirectionalInput.magnitude * 50;
            
            GUI.DrawTexture(new Rect(inputStatePos, new Vector2(100, 100)),
                unitCircleTexture);
            GUI.DrawTexture(new Rect(inputStatePos + markerOffset + currentInput, 
                    new(10, 10)), directionalInputMarkerTexture);
            
            GUI.Label(new Rect (inputStatePos + new Vector2(0, 105), new Vector2(300, 20)), 
                CharacterInput.InputState.DirectionalInput.ToString());
            GUI.Toggle(new Rect(inputStatePos + new Vector2(0, 120),
                    new Vector2(200, 20)), 
                CharacterInput.InputState.DashButton == InputActionPhase.Performed, "DASH");
        }
    }
}
